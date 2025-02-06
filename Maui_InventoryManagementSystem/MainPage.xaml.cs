using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;

namespace Maui_InventoryManagementSystem
{
    public partial class MainPage : ContentPage
    {
        private string connectionString = "Server=DESKTOP-70DKLSR\\SQLEXPRESS; Database=InventoryDB; Integrated Security=True;TrustServerCertificate=True;";
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        private Product? _selectedProduct;

        public MainPage()
        {
            InitializeComponent();
            ProductListView.ItemsSource = Products;
            LoadProducts();
        }

        private void LoadProducts(string searchTerm = "")
        {
            try
            {
                searchTerm = searchTerm ?? string.Empty;
                Products.Clear();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "Select * From Products Where Name Like @SearchTerm OR Category Like @SearchTerm";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Products.Add(new Product
                                {
                                    ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
                                });
                            }
                        }
                    }
                }
            }
            catch(SqlException ex)
            {
                DisplayAlert("Error", $"Database error: {ex.Message}", "OK");
            }
            catch(Exception ex)
            {
                DisplayAlert("Error", $"Error occured: {ex.Message}", "OK");
            }
            
        }

        private void OnAddProductClicked(object sender, EventArgs e)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "Insert Into Products(Name, Category, Price, Stock) Values (@Name, @Category, @Price, @Stock)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", NameEntry.Text);
                        command.Parameters.AddWithValue("@Category", CategoryEntry.Text);
                        command.Parameters.AddWithValue("@Price", decimal.Parse(PriceEntry.Text));
                        command.Parameters.AddWithValue("@Stock", int.Parse(StockEntry.Text));

                        command.ExecuteNonQuery();
                    }
                }
                ClearEntries();
                LoadProducts();
                DisplayAlert("Success", "Product added successfully", "OK");
            }
            catch (SqlException ex)
            {
                DisplayAlert("Error", $"Input error: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Invalid error: {ex.Message}", "OK");
            }
        }

        private void OnUpdateProductClicked(object sender, EventArgs e)
        {
            try
            {
                if (_selectedProduct != null && ValidateInputs())
                {
                    using (var connecton = new SqlConnection(connectionString))
                    {
                        connecton.Open();
                        var query = "Update Products Set Name = @Name, Category = @Category, Price = @Price, Stock = @Stock Where ProductID = @ProductID";

                        using (var command = new SqlCommand(query, connecton))
                        {
                            command.Parameters.AddWithValue("@Name", NameEntry.Text);
                            command.Parameters.AddWithValue("@Category", CategoryEntry.Text);
                            command.Parameters.AddWithValue("@Price", decimal.Parse(PriceEntry.Text));
                            command.Parameters.AddWithValue("@Stock", int.Parse(StockEntry.Text));
                            command.Parameters.AddWithValue("@ProductID", _selectedProduct.ProductID);

                            command.ExecuteNonQuery();
                        }
                    }
                    LoadProducts();
                    ClearEntries();
                    _selectedProduct = null;
                    DisplayAlert("Success", "Product updated successfully", "OK");
                }
            }
            catch (SqlException ex)
            {
                DisplayAlert("Error", $"Input error: {ex.Message}", "OK");
            }
            catch(Exception ex)
            {
                DisplayAlert("Error", $"Invalid error: {ex.Message}", "OK");
            }
           
           
        }

        private void OnSearchProductTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = e.NewTextValue; // Get the new text from the search box
            LoadProducts(SearchProduct.Text);
        }

        private void OnEditProductClicked(object sender, EventArgs e)
        {
            if(sender is Button button && button.CommandParameter is Product product)
            {
                _selectedProduct = product;
                NameEntry.Text = product.Name;
                CategoryEntry.Text = product.Category;
                PriceEntry.Text = product.Price.ToString();
                StockEntry.Text = product.Stock.ToString();
            }
        }

        private void OnDeleteProductClicked(object sender, EventArgs e)
        {
            if(sender is Button button && button.CommandParameter is Product product)
            {
                using(var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "Delete From Products Where ProductID = @ProductID";
                    using(var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", product.ProductID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            LoadProducts();
        }

        private void ClearEntries()
        {
            NameEntry.Text = string.Empty;
            CategoryEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
            StockEntry.Text = string.Empty;
        }

        private bool ValidateInputs()
        {
            //Check for empty fields and valid input values
            if(string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(CategoryEntry.Text))
            {
                DisplayAlert("Error", "Name and Category are empty", "OK");
                return false;
            }

            if(!decimal.TryParse(PriceEntry.Text, out decimal price) || price <= 0)
            {
                DisplayAlert("Error", "Invalid price. Enter a positive number", "OK");
                return false;
            }

            if(!int.TryParse(StockEntry.Text, out int stock) || stock <= 0)
            {
                DisplayAlert("Error", "Invalid stock. Enter non negative stock", "OK");
                return false;
            }
            return true;
        }
    }

}
