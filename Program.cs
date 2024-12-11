using System;
using Gtk;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

public class GasStationManagementApp : Window
{
    private SqlConnection connection;
    private Label petrolStationInfo;

    public GasStationManagementApp() : base("Gas Station Management")
    {
        DeleteEvent += OnDeleteEvent;
        // Initialize database connection
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=benz;Integrated Security=True;";
        connection = new SqlConnection(connectionString);
        connection.Open();

        // Setup main window
        SetDefaultSize(1024/3, 768/2);
        SetPosition(WindowPosition.Center);

        VBox mainLayout = new VBox();
        Add(mainLayout);

        // Top bar for petrol station info
        petrolStationInfo = new Label("Loading station data...");
        mainLayout.PackStart(petrolStationInfo, false, false, 10);
        RefreshPetrolStationInfo();

        // Side menu with buttons
        VBox menu = new VBox();
        mainLayout.PackStart(menu, false, false, 10);

        Button inOutTransactionsButton = new Button("In/Out Transactions");
        inOutTransactionsButton.Clicked += OnInOutTransactionsClicked;

        Button receiptsButton = new Button("Receipts");
        receiptsButton.Clicked += OnReceiptManagementClicked;

        Button supplementsButton = new Button("Supplements");
        supplementsButton.Clicked += OnSupplementManagementClicked;

        Button editPetrolStationButton = new Button("Edit Petrol Station Data");
        editPetrolStationButton.Clicked += OnEditPetrolStationClicked;

        Button suppliers = new Button("Manage Suppliers");
        suppliers.Clicked += OnSuppliersClicked;

        Button priceListButton = new Button("Price List Management");
        priceListButton.Clicked += OnPriceListManagementClicked;
        menu.PackStart(priceListButton, false, false, 5);


        menu.PackStart(inOutTransactionsButton, false, false, 5);
        menu.PackStart(receiptsButton, false, false, 5);
        menu.PackStart(supplementsButton, false, false, 5);
        menu.PackStart(editPetrolStationButton, false, false, 5);
        menu.PackStart(suppliers, false, false, 5);

        CheckFirstTimeSetup();

        ShowAll();
    }

    private void OnDeleteEvent(object sender, DeleteEventArgs args)
    {
        // Properly close the database connection if open
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
            connection.Dispose();
        }

        // Exit the GTK application
        Application.Quit();

        // Indicate the event has been handled
        args.RetVal = true;
    }

    private void RefreshPetrolStationInfo()
    {
        string query = "SELECT TOP 1 * FROM PetrolStation";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    petrolStationInfo.Text = $@"
Station Name: {reader["Name"]}
Address: {reader["Address"]}
Contact: {reader["ContactNumber"]}
Gas: {reader["Gas"]} L
Petrol: {reader["Petrol"]} L
LMG: {reader["LMG"]} L
Money Made: {reader["MoneyMade"]:C}";
                }
                else
                {
                    petrolStationInfo.Text = "No petrol station data available.";
                }
            }
        }
    }

    private void CheckFirstTimeSetup()
    {
        string query = "SELECT COUNT(*) FROM PetrolStation";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            int count = (int)command.ExecuteScalar();
            if (count == 0)
            {
                ShowFirstTimeSetupDialog();
            }
        }
    }

    private void ShowFirstTimeSetupDialog()
    {
        Dialog dialog = new Dialog("First Time Setup", this, DialogFlags.Modal);
        dialog.SetDefaultSize(400, 300);

        VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        Entry nameEntry = new Entry { PlaceholderText = "Station Name" };
        Entry addressEntry = new Entry { PlaceholderText = "Address" };
        Entry contactEntry = new Entry { PlaceholderText = "Contact Number" };

        Button saveButton = new Button("Save");
        saveButton.Clicked += (sender, args) =>
        {
            string name = nameEntry.Text;
            string address = addressEntry.Text;
            string contact = contactEntry.Text;

            string insertQuery = "INSERT INTO PetrolStation (Name, Address, ContactNumber, Gas, Petrol, LMG, MoneyMade) VALUES (@Name, @Address, @ContactNumber, 0, 0, 0, 0)";
            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@ContactNumber", contact);
                command.ExecuteNonQuery();
            }

            dialog.Destroy();
            RefreshPetrolStationInfo();
        };

        dialogBox.PackStart(new Label("Enter Petrol Station Details:"), false, false, 5);
        dialogBox.PackStart(nameEntry, false, false, 5);
        dialogBox.PackStart(addressEntry, false, false, 5);
        dialogBox.PackStart(contactEntry, false, false, 5);
        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
    }
    VBox supplierList = new VBox();
    private void OnSuppliersClicked(object sender, EventArgs e)
    {
        // Create a new dialog window for managing suppliers
        Dialog suppliersDialog = new Dialog("Manage Suppliers", this, DialogFlags.Modal);
        suppliersDialog.SetDefaultSize(600, 400);

        VBox dialogBox = new VBox();
        suppliersDialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Add Supplier Button
        Button addSupplierButton = new Button("Add Supplier");
        addSupplierButton.Clicked += OnAddSupplierClicked;  // Handles the click event for adding a supplier
        dialogBox.PackStart(addSupplierButton, false, false, 5);

        // Scrolled window for the supplier list
        ScrolledWindow scroll = new ScrolledWindow();

        // This is the container where the supplier list will be populated

        scroll.Add(supplierList);
        dialogBox.PackStart(scroll, true, true, 10);

        // Populate the supplier list from the database
        PopulateSupplierList(supplierList);

        // Show all dialog elements
        suppliersDialog.ShowAll();
    }

    VBox supplementList = new VBox();

    private void OnSupplementManagementClicked(object sender, EventArgs e)
    {
        // Create a new dialog window for managing supplements
        Dialog supplementDialog = new Dialog("Manage Supplements", this, DialogFlags.Modal);
        supplementDialog.SetDefaultSize(600, 400);

        VBox dialogBox = new VBox();
        supplementDialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Add Supplement Button
        Button addSupplementButton = new Button("Add Supplement");
        addSupplementButton.Clicked += OnAddSupplementClicked;
        dialogBox.PackStart(addSupplementButton, false, false, 5);

        // Scrolled window for supplement list
        ScrolledWindow scroll = new ScrolledWindow();
        scroll.Add(supplementList);
        dialogBox.PackStart(scroll, true, true, 10);

        // Populate the supplement list
        PopulateSupplementList(supplementList);

        supplementDialog.ShowAll();
    }

    private void PopulateSupplementList(VBox supplementList)
    {
        // Clear any existing supplement list items
        supplementList.Children.ToList().ForEach(widget => supplementList.Remove(widget));

        // Query the database for supplements
        string query = "SELECT ID, GasQuantity, PetrolQuantity, LMGQuantity, GasPrice, PetrolPrice, LMGPrice, SupplementDate FROM Supplement WHERE PetrolStationID = 1";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    HBox supplementRow = new HBox();

                    decimal gasQuantity = (decimal)reader["GasQuantity"];
                    decimal petrolQuantity = (decimal)reader["PetrolQuantity"];
                    decimal lmgQuantity = (decimal)reader["LMGQuantity"];
                    decimal gasPrice = (decimal)reader["GasPrice"];
                    decimal petrolPrice = (decimal)reader["PetrolPrice"];
                    decimal lmgPrice = (decimal)reader["LMGPrice"];
                    DateTime supplementDate = (DateTime)reader["SupplementDate"];
                    int id = (int)reader["ID"];

                    // Calculate total cost of the supplement
                    decimal totalCost = (gasQuantity * gasPrice) + (petrolQuantity * petrolPrice) + (lmgQuantity * lmgPrice);

                    Label supplementInfo = new Label($"{supplementDate.ToShortDateString()} | Gas: {gasQuantity}L | Petrol: {petrolQuantity}L | LMG: {lmgQuantity}L | Total: ${totalCost}");
                    Button editButton = new Button("Edit");
                    Button deleteButton = new Button("Delete");

                    editButton.Clicked += (sender, args) => ShowEditSupplementDialog(id, gasQuantity, petrolQuantity, lmgQuantity, gasPrice, petrolPrice, lmgPrice, supplementDate);
                    deleteButton.Clicked += (sender, args) => DeleteSupplement(id);

                    supplementRow.PackStart(supplementInfo, true, true, 5);
                    supplementRow.PackStart(editButton, false, false, 5);
                    supplementRow.PackStart(deleteButton, false, false, 5);

                    supplementList.PackStart(supplementRow, false, false, 5);
                }
            }
        }

        // Force GTK to refresh the UI
        supplementList.ShowAll();
    }

    private void OnAddSupplementClicked(object sender, EventArgs args)
    {
        // Create a dialog for adding a new supplement
        Dialog dialog = new Dialog("Add Supplement", this, DialogFlags.Modal);
        dialog.SetDefaultSize(400, 500);

            VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Supplier dropdown
        Label supplierLabel = new Label("Select Supplier:");
        ComboBoxText supplierComboBox = new ComboBoxText();
        PopulateSuppliers(supplierComboBox);

        // Input fields for Gas, Petrol, and LMG quantities and prices
        Label gasQuantityLabel = new Label("Gas Quantity (L):");
        SpinButton gasQuantityEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label petrolQuantityLabel = new Label("Petrol Quantity (L):");
        SpinButton petrolQuantityEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label lmgQuantityLabel = new Label("LMG Quantity (L):");
        SpinButton lmgQuantityEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label gasPriceLabel = new Label("Gas Price per Liter:");
        SpinButton gasPriceEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label petrolPriceLabel = new Label("Petrol Price per Liter:");
        SpinButton petrolPriceEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label lmgPriceLabel = new Label("LMG Price per Liter:");
        SpinButton lmgPriceEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        // Add date picker for the supplement date
        Label dateLabel = new Label("Supplement Date:");
        Calendar supplementDatePicker = new Calendar();

        // Save button to submit the new supplement
        Button saveButton = new Button("Save");

        saveButton.Clicked += (s, e) =>
        {
            string selectedSupplier = supplierComboBox.ActiveText;
            if (selectedSupplier == null)
            {
                MessageDialog noSupplierDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Please select a supplier.");
                noSupplierDialog.Run();
                noSupplierDialog.Destroy();
                return;
            }

            decimal gasQuantity = (decimal)gasQuantityEntry.Value;
            decimal petrolQuantity = (decimal)petrolQuantityEntry.Value;
            decimal lmgQuantity = (decimal)lmgQuantityEntry.Value;
            decimal gasPrice = (decimal)gasPriceEntry.Value;
            decimal petrolPrice = (decimal)petrolPriceEntry.Value;
            decimal lmgPrice = (decimal)lmgPriceEntry.Value;

            // Get selected date from the date picker
            DateTime supplementDate = new DateTime((int)supplementDatePicker.Date.Year,
                                                   (int)supplementDatePicker.Date.Month,
                                                   (int)supplementDatePicker.Date.Day);

            // Calculate the total cost
            decimal totalCost = (gasQuantity * gasPrice) + (petrolQuantity * petrolPrice) + (lmgQuantity * lmgPrice);

            // Check if the petrol station has enough money
            string balanceQuery = "SELECT MoneyMade FROM PetrolStation WHERE ID = 1";
            decimal availableBalance = (decimal)new SqlCommand(balanceQuery, connection).ExecuteScalar();

            if (totalCost > availableBalance)
            {
                MessageDialog insufficientFundsDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Insufficient funds to process this supplement.");
                insufficientFundsDialog.Run();
                insufficientFundsDialog.Destroy();
                return;
            }

            string selectedText = supplierComboBox.ActiveText;
            string pattern = @"\d+"; // Matches one or more digits

            // Find the first match
            Match match = Regex.Match(selectedText, pattern);

            int supplierID = 0;

            if (match.Success)
            {
                supplierID = int.Parse(match.Value); // Convert matched number to int
            }

            // Use the stored procedure to add the supplement to the database
            string procedure = "EXEC AddFuelSupply @PetrolStationID, @SupplierID, @GasQuantity, @PetrolQuantity, @LMGQuantity, @GasPrice, @PetrolPrice, @LMGPrice, @SupplementDate";
            using (SqlCommand command = new SqlCommand(procedure, connection))
            {
                command.Parameters.AddWithValue("@PetrolStationID", 1);
                command.Parameters.AddWithValue("@SupplierID", supplierID); // Selected supplier
                command.Parameters.AddWithValue("@GasQuantity", gasQuantity);
                command.Parameters.AddWithValue("@PetrolQuantity", petrolQuantity);
                command.Parameters.AddWithValue("@LMGQuantity", lmgQuantity);
                command.Parameters.AddWithValue("@GasPrice", gasPrice);
                command.Parameters.AddWithValue("@PetrolPrice", petrolPrice);
                command.Parameters.AddWithValue("@LMGPrice", lmgPrice);
                command.Parameters.AddWithValue("@SupplementDate", supplementDate);
                command.ExecuteNonQuery();
            }

            // Close the dialog and refresh the supplement list
            dialog.Destroy();
            PopulateSupplementList(supplementList);
        };

        // Add dropdown, input fields, and save button to the dialog
        dialogBox.PackStart(supplierLabel, false, false, 5);
        dialogBox.PackStart(supplierComboBox, false, false, 5);

        dialogBox.PackStart(gasQuantityLabel, false, false, 5);
        dialogBox.PackStart(gasQuantityEntry, false, false, 5);

        dialogBox.PackStart(petrolQuantityLabel, false, false, 5);
        dialogBox.PackStart(petrolQuantityEntry, false, false, 5);

        dialogBox.PackStart(lmgQuantityLabel, false, false, 5);
        dialogBox.PackStart(lmgQuantityEntry, false, false, 5);

        dialogBox.PackStart(gasPriceLabel, false, false, 5);
        dialogBox.PackStart(gasPriceEntry, false, false, 5);

        dialogBox.PackStart(petrolPriceLabel, false, false, 5);
        dialogBox.PackStart(petrolPriceEntry, false, false, 5);

        dialogBox.PackStart(lmgPriceLabel, false, false, 5);
        dialogBox.PackStart(lmgPriceEntry, false, false, 5);

        dialogBox.PackStart(dateLabel, false, false, 5);
        dialogBox.PackStart(supplementDatePicker, false, false, 5);

        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
        RefreshPetrolStationInfo();
    }


    private void PopulateSuppliers(ComboBoxText supplierDropdown)
    {
        string query = "SELECT ID, Name FROM Supplier";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["ID"];
                    string name = (string)reader["Name"];
                    supplierDropdown.AppendText($"{id} - {name}");
                }
            }
        }
    }


    private void ShowEditSupplementDialog(int id, decimal oldGasQuantity, decimal oldPetrolQuantity, decimal oldLMGQuantity,
decimal oldGasPrice, decimal oldPetrolPrice, decimal oldLMGPrice, DateTime oldSupplementDate)
    {
        Dialog editDialog = new Dialog("Edit Supplement", this, DialogFlags.Modal);
        VBox dialogContentArea = new VBox();
        editDialog.ContentArea.PackStart(dialogContentArea, true, true, 0);

        // Date picker for supplement date
        Label dateLabel = new Label("Supplement Date:");
        Calendar datePicker = new Calendar();
        datePicker.Date = oldSupplementDate;

        // Input fields for quantities and prices
        Label gasQuantityLabel = new Label("Gas Quantity (L):");
        SpinButton gasQuantityEntry = new SpinButton(0, 10000, 0.01) { Value = (double)oldGasQuantity };

        Label petrolQuantityLabel = new Label("Petrol Quantity (L):");
        SpinButton petrolQuantityEntry = new SpinButton(0, 10000, 0.01) { Value = (double)oldPetrolQuantity };

        Label lmgQuantityLabel = new Label("LMG Quantity (L):");
        SpinButton lmgQuantityEntry = new SpinButton(0, 10000, 0.01) { Value = (double)oldLMGQuantity };

        Label gasPriceLabel = new Label("Gas Price per Liter:");
        SpinButton gasPriceEntry = new SpinButton(0, 10000, 0.01) { Value = (double)oldGasPrice };

        Label petrolPriceLabel = new Label("Petrol Price per Liter:");
        SpinButton petrolPriceEntry = new SpinButton(0, 10000, 0.01) { Value = (double)oldPetrolPrice };

        Label lmgPriceLabel = new Label("LMG Price per Liter:");
        SpinButton lmgPriceEntry = new SpinButton(0, 10000, 0.01) { Value = (double)oldLMGPrice };

        // Save button for updating the supplement
        Button saveButton = new Button("Save");
        saveButton.Clicked += (sender, e) =>
        {
            decimal newGasQuantity = (decimal)gasQuantityEntry.Value;
            decimal newPetrolQuantity = (decimal)petrolQuantityEntry.Value;
            decimal newLMGQuantity = (decimal)lmgQuantityEntry.Value;
            decimal newGasPrice = (decimal)gasPriceEntry.Value;
            decimal newPetrolPrice = (decimal)petrolPriceEntry.Value;
            decimal newLMGPrice = (decimal)lmgPriceEntry.Value;

            DateTime newSupplementDate = datePicker.Date;

            // Calculate differences in fuel quantities and prices
            decimal gasDifference = newGasQuantity - oldGasQuantity;
            decimal petrolDifference = newPetrolQuantity - oldPetrolQuantity;
            decimal lmgDifference = newLMGQuantity - oldLMGQuantity;

            decimal oldTotalCost = (oldGasQuantity * oldGasPrice) + (oldPetrolQuantity * oldPetrolPrice) + (oldLMGQuantity * oldLMGPrice);
            decimal newTotalCost = (newGasQuantity * newGasPrice) + (newPetrolQuantity * newPetrolPrice) + (newLMGQuantity * newLMGPrice);
            decimal costDifference = newTotalCost - oldTotalCost;

            // Check if sufficient funds are available for increased costs
            if (costDifference > 0)
            {
                string balanceQuery = "SELECT MoneyMade FROM PetrolStation WHERE ID = 1";
                decimal availableBalance = (decimal)new SqlCommand(balanceQuery, connection).ExecuteScalar();

                if (costDifference > availableBalance)
                {
                    MessageDialog insufficientFundsDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Insufficient funds to update this supplement.");
                    insufficientFundsDialog.Run();
                    insufficientFundsDialog.Destroy();
                    return;
                }
            }

            // Update the PetrolStation fuel quantities and balance
            string updateStationQuery = "UPDATE PetrolStation SET Gas = Gas + @GasDifference, Petrol = Petrol + @PetrolDifference, LMG = LMG + @LMGDifference, MoneyMade = MoneyMade - @CostDifference WHERE ID = 1";
            using (SqlCommand command = new SqlCommand(updateStationQuery, connection))
            {
                command.Parameters.AddWithValue("@GasDifference", gasDifference);
                command.Parameters.AddWithValue("@PetrolDifference", petrolDifference);
                command.Parameters.AddWithValue("@LMGDifference", lmgDifference);
                command.Parameters.AddWithValue("@CostDifference", costDifference);
                command.ExecuteNonQuery();
            }

            // Update supplement in the database
            string updateQuery = "UPDATE Supplement SET GasQuantity = @GasQuantity, PetrolQuantity = @PetrolQuantity, LMGQuantity = @LMGQuantity, " +
                                 "GasPrice = @GasPrice, PetrolPrice = @PetrolPrice, LMGPrice = @LMGPrice, SupplementDate = @SupplementDate WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@GasQuantity", newGasQuantity);
                command.Parameters.AddWithValue("@PetrolQuantity", newPetrolQuantity);
                command.Parameters.AddWithValue("@LMGQuantity", newLMGQuantity);
                command.Parameters.AddWithValue("@GasPrice", newGasPrice);
                command.Parameters.AddWithValue("@PetrolPrice", newPetrolPrice);
                command.Parameters.AddWithValue("@LMGPrice", newLMGPrice);
                command.Parameters.AddWithValue("@SupplementDate", newSupplementDate);
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteNonQuery();
            }

            // Close the dialog and refresh the list
            editDialog.Destroy();
            PopulateSupplementList(supplementList);
        };

        // Add widgets to dialog
        dialogContentArea.PackStart(dateLabel, false, false, 5);
        dialogContentArea.PackStart(datePicker, false, false, 5);

        dialogContentArea.PackStart(gasQuantityLabel, false, false, 5);
        dialogContentArea.PackStart(gasQuantityEntry, false, false, 5);

        dialogContentArea.PackStart(petrolQuantityLabel, false, false, 5);
        dialogContentArea.PackStart(petrolQuantityEntry, false, false, 5);

        dialogContentArea.PackStart(lmgQuantityLabel, false, false, 5);
        dialogContentArea.PackStart(lmgQuantityEntry, false, false, 5);

        dialogContentArea.PackStart(gasPriceLabel, false, false, 5);
        dialogContentArea.PackStart(gasPriceEntry, false, false, 5);

        dialogContentArea.PackStart(petrolPriceLabel, false, false, 5);
        dialogContentArea.PackStart(petrolPriceEntry, false, false, 5);

        dialogContentArea.PackStart(lmgPriceLabel, false, false, 5);
        dialogContentArea.PackStart(lmgPriceEntry, false, false, 5);

        dialogContentArea.PackStart(saveButton, false, false, 5);

        editDialog.ShowAll();
        RefreshPetrolStationInfo();
    }


    private void DeleteSupplement(int id)
    {
        // Retrieve the supplement's total cost and quantities
        string costAndQuantityQuery = "SELECT (GasQuantity * GasPrice) + (PetrolQuantity * PetrolPrice) + (LMGQuantity * LMGPrice) AS TotalCost, GasQuantity, PetrolQuantity, LMGQuantity FROM Supplement WHERE ID = @ID";
        decimal supplementCost;
        decimal gasQuantity, petrolQuantity, lmgQuantity;

        using (SqlCommand command = new SqlCommand(costAndQuantityQuery, connection))
        {
            command.Parameters.AddWithValue("@ID", id);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    supplementCost = reader.GetDecimal(0);
                    gasQuantity = reader.GetDecimal(1);
                    petrolQuantity = reader.GetDecimal(2);
                    lmgQuantity = reader.GetDecimal(3);
                }
                else
                {
                    throw new Exception("Supplement not found.");
                }
            }
        }

        // Adjust the available balance
        string updateBalanceQuery = "UPDATE PetrolStation SET MoneyMade = MoneyMade + @Cost WHERE ID = 1";
        using (SqlCommand command = new SqlCommand(updateBalanceQuery, connection))
        {
            command.Parameters.AddWithValue("@Cost", supplementCost);
            command.ExecuteNonQuery();
        }

        // Adjust the fuel quantities
        string updateFuelQuery = "UPDATE PetrolStation SET Gas = Gas - @GasQuantity, Petrol = Petrol - @PetrolQuantity, LMG = LMG - @LMGQuantity WHERE ID = 1";
        using (SqlCommand command = new SqlCommand(updateFuelQuery, connection))
        {
            command.Parameters.AddWithValue("@GasQuantity", gasQuantity);
            command.Parameters.AddWithValue("@PetrolQuantity", petrolQuantity);
            command.Parameters.AddWithValue("@LMGQuantity", lmgQuantity);
            command.ExecuteNonQuery();
        }

        // Delete the supplement
        string deleteQuery = "DELETE FROM Supplement WHERE ID = @ID";
        using (SqlCommand command = new SqlCommand(deleteQuery, connection))
        {
            command.Parameters.AddWithValue("@ID", id);
            command.ExecuteNonQuery();
        }

        // Refresh the supplement list
        PopulateSupplementList(supplementList);
        RefreshPetrolStationInfo();
    }



    VBox transactionList = new VBox();

    private void OnInOutTransactionsClicked(object sender, EventArgs e)
    {
        // Create a new dialog window for In/Out Transactions
        Dialog transactionsDialog = new Dialog("In/Out Transactions", this, DialogFlags.Modal);
        transactionsDialog.SetDefaultSize(600, 400);

        VBox dialogBox = new VBox();
        transactionsDialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Add Transaction Button
        Button addTransactionButton = new Button("Add Transaction");
        addTransactionButton.Clicked += OnAddTransactionClicked;
        dialogBox.PackStart(addTransactionButton, false, false, 5);

        // Scrolled window for transaction list
        ScrolledWindow scroll = new ScrolledWindow();
        
        scroll.Add(transactionList);
        dialogBox.PackStart(scroll, true, true, 10);

        // Populate the transaction list
        PopulateTransactionList(transactionList);

        transactionsDialog.ShowAll();
        RefreshPetrolStationInfo();
    }

    private void PopulateTransactionList(VBox transactionList)
    {
        // Clear any existing transaction list items
        transactionList.Children.ToList().ForEach(widget => transactionList.Remove(widget));

        // Query the database for transactions
        string query = "SELECT ID, TransactionType, Amount, TransactionDate FROM OutsideTransactions WHERE PetrolStationID = 1";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    HBox transactionRow = new HBox();

                    string transactionType = reader["TransactionType"].ToString();
                    decimal amount = (decimal)reader["Amount"];
                    DateTime date = (DateTime)reader["TransactionDate"];
                    int id = (int)reader["ID"];

                    Label transactionInfo = new Label($"{transactionType} | ${amount} | {date.ToShortDateString()}");
                    Button editButton = new Button("Edit");
                    Button deleteButton = new Button("Delete");

                    editButton.Clicked += (sender, args) => ShowEditTransactionDialog(id, transactionType, amount, date);
                    deleteButton.Clicked += (sender, args) => DeleteTransaction(id, transactionType, amount);

                    transactionRow.PackStart(transactionInfo, true, true, 5);
                    transactionRow.PackStart(editButton, false, false, 5);
                    transactionRow.PackStart(deleteButton, false, false, 5);

                    transactionList.PackStart(transactionRow, false, false, 5);
                }
            }
        }

        // Force GTK to refresh the UI
        transactionList.ShowAll();
        RefreshPetrolStationInfo();
    }




    private void OnAddTransactionClicked(object sender, EventArgs args)
    {
        // Create a dialog for adding a new transaction
        Dialog dialog = new Dialog("Add Transaction", this, DialogFlags.Modal);
        dialog.SetDefaultSize(400, 300);

        VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Input fields for transaction type and amount
        Entry transactionTypeEntry = new Entry { PlaceholderText = "Transaction Type" };
        Entry amountEntry = new Entry { PlaceholderText = "Amount" };

        // Save button to submit the new transaction
        Button saveButton = new Button("Save");
        saveButton.Clicked += (s, e) =>
        {
            string transactionType = transactionTypeEntry.Text;
            decimal amount = decimal.Parse(amountEntry.Text);
            DateTime transactionDate = DateTime.Now;

            // Use the stored procedure to add the transaction to the database
            string procedure = "EXEC ProcessOutsideTransaction @PetrolStationID, @TransactionType, @Amount, @TransactionDate";
            using (SqlCommand command = new SqlCommand(procedure, connection))
            {
                command.Parameters.AddWithValue("@PetrolStationID", 1); // Assuming a single petrol station
                command.Parameters.AddWithValue("@TransactionType", transactionType);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@TransactionDate", transactionDate);

                command.ExecuteNonQuery();
            }

            // Close the dialog and refresh the transaction list
            dialog.Destroy();
            PopulateTransactionList(transactionList);
        };


        // Add input fields and the save button to the dialog
        dialogBox.PackStart(new Label("Enter Transaction Details"), false, false, 5);
        dialogBox.PackStart(transactionTypeEntry, false, false, 5);
        dialogBox.PackStart(amountEntry, false, false, 5);
        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
        RefreshPetrolStationInfo();
    }


    private void ShowEditTransactionDialog(int id, string oldTransactionType, decimal oldAmount, DateTime oldDate)
    {
        Dialog editDialog = new Dialog("Edit Transaction", this, DialogFlags.Modal);
        Box dialogContentArea = editDialog.ContentArea; // Correct type is Gtk.Box, not Gtk.VBox

        Entry transactionTypeEntry = new Entry { Text = oldTransactionType };
        SpinButton amountEntry = new SpinButton(0, 1000000, 1) { Value = (double)oldAmount };
        Calendar dateEntry = new Calendar();
        dateEntry.SelectDay((uint)oldDate.Day);
        dateEntry.SelectMonth((uint)oldDate.Month - 1, (uint)oldDate.Year);

        dialogContentArea.PackStart(new Label("Transaction Type:"), false, false, 5);
        dialogContentArea.PackStart(transactionTypeEntry, false, false, 5);
        dialogContentArea.PackStart(new Label("Amount:"), false, false, 5);
        dialogContentArea.PackStart(amountEntry, false, false, 5);
        dialogContentArea.PackStart(new Label("Date:"), false, false, 5);
        dialogContentArea.PackStart(dateEntry, false, false, 5);

        Button okButton = new Button("OK");
        okButton.Clicked += (sender, e) =>
        {
            string newTransactionType = transactionTypeEntry.Text;
            decimal newAmount = (decimal)amountEntry.Value;
            DateTime newDate = dateEntry.Date;

            // Update the database
            string updateQuery = "UPDATE OutsideTransactions SET TransactionType = @TransactionType, Amount = @Amount, TransactionDate = @TransactionDate WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@TransactionType", newTransactionType);
                command.Parameters.AddWithValue("@Amount", newAmount);
                command.Parameters.AddWithValue("@TransactionDate", newDate);
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteNonQuery();
            }

            // Adjust the balance
            decimal difference = newAmount - oldAmount;

            if (oldTransactionType == "Investment" || oldTransactionType == "Development Investment")
            {
                difference *= 1; // Investments are inflow
            }
            else
            {
                difference *= -1; // Other types are outflow
            }

            string balanceUpdateQuery = "UPDATE PetrolStation SET MoneyMade = MoneyMade + @Difference WHERE ID = 1";
            using (SqlCommand balanceCommand = new SqlCommand(balanceUpdateQuery, connection))
            {
                balanceCommand.Parameters.AddWithValue("@Difference", difference);
                balanceCommand.ExecuteNonQuery();
            }

            // Refresh the transaction list
            PopulateTransactionList(transactionList);
            editDialog.Destroy();
        };

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += (sender, e) => editDialog.Destroy();

        editDialog.ActionArea.PackStart(okButton, false, false, 5);
        editDialog.ActionArea.PackStart(cancelButton, false, false, 5);

        editDialog.ShowAll();
        RefreshPetrolStationInfo();
    }


    private void DeleteTransaction(int id, string transactionType, decimal amount)
    {
        string query = "DELETE FROM OutsideTransactions WHERE ID = @ID";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@ID", id);
            command.ExecuteNonQuery();
        }

        UpdateBalance(amount, transactionType, isDeleting: true);
        PopulateTransactionList(transactionList);
        RefreshPetrolStationInfo();
    }

    private void PopulateSupplierList(VBox supplierList)
    {
        // Clear any existing supplier list items
        supplierList.Children.ToList().ForEach(widget => supplierList.Remove(widget));

        // Query the database for suppliers
        string query = "SELECT ID, Name, Address, ContactNumber, Email FROM Supplier";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    HBox supplierRow = new HBox();

                    string name = reader["Name"].ToString();
                    string address = reader["Address"].ToString();
                    string contactNumber = reader["ContactNumber"].ToString();
                    string email = reader["Email"].ToString();
                    int id = (int)reader["ID"];

                    Label supplierInfo = new Label($"{name} | {address} | {contactNumber} | {email}");
                    Button editButton = new Button("Edit");
                    Button deleteButton = new Button("Delete");

                    editButton.Clicked += (sender, args) => ShowEditSupplierDialog(id, name, address, contactNumber, email);
                    deleteButton.Clicked += (sender, args) => DeleteSupplier(id);

                    supplierRow.PackStart(supplierInfo, true, true, 5);
                    supplierRow.PackStart(editButton, false, false, 5);
                    supplierRow.PackStart(deleteButton, false, false, 5);

                    supplierList.PackStart(supplierRow, false, false, 5);
                }
            }
        }

        // Force GTK to refresh the UI
        supplierList.ShowAll();
    }



    private void OnAddSupplierClicked(object sender, EventArgs args)
    {
        // Create a dialog for adding a new supplier
        Dialog dialog = new Dialog("Add Supplier", this, DialogFlags.Modal);
        dialog.SetDefaultSize(400, 300);

        VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Input fields for supplier details
        Entry nameEntry = new Entry { PlaceholderText = "Supplier Name" };
        Entry addressEntry = new Entry { PlaceholderText = "Supplier Address" };
        Entry contactNumberEntry = new Entry { PlaceholderText = "Contact Number" };
        Entry emailEntry = new Entry { PlaceholderText = "Email" };

        // Save button to submit the new supplier
        Button saveButton = new Button("Save");
        saveButton.Clicked += (s, e) =>
        {
            string name = nameEntry.Text;
            string address = addressEntry.Text;
            string contactNumber = contactNumberEntry.Text;
            string email = emailEntry.Text;

            // Insert the new supplier into the database
            string query = "INSERT INTO Supplier (Name, Address, ContactNumber, Email) VALUES (@Name, @Address, @ContactNumber, @Email)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@ContactNumber", contactNumber);
                command.Parameters.AddWithValue("@Email", email);
                command.ExecuteNonQuery();
            }

            // Close the dialog and refresh the supplier list
            dialog.Destroy();
            PopulateSupplierList(supplierList);  // Refresh the supplier list
        };

        // Add input fields and the save button to the dialog
        dialogBox.PackStart(new Label("Enter Supplier Details"), false, false, 5);
        dialogBox.PackStart(nameEntry, false, false, 5);
        dialogBox.PackStart(addressEntry, false, false, 5);
        dialogBox.PackStart(contactNumberEntry, false, false, 5);
        dialogBox.PackStart(emailEntry, false, false, 5);
        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
    }



    private void ShowEditSupplierDialog(int id, string oldName, string oldAddress, string oldContactNumber, string oldEmail)
    {
        Dialog editDialog = new Dialog("Edit Supplier", this, DialogFlags.Modal);
        VBox dialogContentArea = new VBox();
        editDialog.ContentArea.PackStart(dialogContentArea, true, true, 0);

        // Pre-fill with the existing supplier data
        Entry nameEntry = new Entry { Text = oldName };
        Entry addressEntry = new Entry { Text = oldAddress };
        Entry contactNumberEntry = new Entry { Text = oldContactNumber };
        Entry emailEntry = new Entry { Text = oldEmail };

        dialogContentArea.PackStart(new Label("Supplier Name:"), false, false, 5);
        dialogContentArea.PackStart(nameEntry, false, false, 5);
        dialogContentArea.PackStart(new Label("Address:"), false, false, 5);
        dialogContentArea.PackStart(addressEntry, false, false, 5);
        dialogContentArea.PackStart(new Label("Contact Number:"), false, false, 5);
        dialogContentArea.PackStart(contactNumberEntry, false, false, 5);
        dialogContentArea.PackStart(new Label("Email:"), false, false, 5);
        dialogContentArea.PackStart(emailEntry, false, false, 5);

        Button okButton = new Button("OK");
        okButton.Clicked += (sender, e) =>
        {
            string newName = nameEntry.Text;
            string newAddress = addressEntry.Text;
            string newContactNumber = contactNumberEntry.Text;
            string newEmail = emailEntry.Text;

            // Update the database with the new supplier details
            string updateQuery = "UPDATE Supplier SET Name = @Name, Address = @Address, ContactNumber = @ContactNumber, Email = @Email WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@Name", newName);
                command.Parameters.AddWithValue("@Address", newAddress);
                command.Parameters.AddWithValue("@ContactNumber", newContactNumber);
                command.Parameters.AddWithValue("@Email", newEmail);
                command.ExecuteNonQuery();
            }

            // Refresh the supplier list after editing
            PopulateSupplierList(supplierList);
            editDialog.Destroy();
        };

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += (sender, e) => editDialog.Destroy();

        editDialog.ActionArea.PackStart(okButton, false, false, 5);
        editDialog.ActionArea.PackStart(cancelButton, false, false, 5);

        editDialog.ShowAll();
    }


    private void DeleteSupplier(int id)
    {
        string query = "DELETE FROM Supplier WHERE ID = @ID";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@ID", id);
            command.ExecuteNonQuery();
        }

        // Refresh the supplier list after deletion
        PopulateSupplierList(supplierList);
    }


    private void UpdateBalance(decimal amount, string transactionType, bool isDeleting = false)
    {
        decimal adjustment = (transactionType == "Investment" || transactionType == "Development Investment")
            ? amount
            : -amount;

        if (isDeleting) adjustment = -adjustment;

        string query = "UPDATE PetrolStation SET MoneyMade = MoneyMade + @Adjustment WHERE ID = 1";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@Adjustment", adjustment);
            command.ExecuteNonQuery();
        }
    }


    private void OpenTransactionsWindow()
    {
        Window transactionsWindow = new Window("Transactions") { DefaultSize = new Gdk.Size(600, 400) };
        VBox layout = new VBox();
        transactionsWindow.Add(layout);

        Button addTransactionButton = new Button("Add Transaction");
        addTransactionButton.Clicked += (s, e) => OpenAddTransactionDialog();
        layout.PackStart(addTransactionButton, false, false, 5);

        ScrolledWindow scroll = new ScrolledWindow();
        VBox transactionList = new VBox();
        scroll.Add(transactionList);
        layout.PackStart(scroll, true, true, 5);

        PopulateTransactionList(transactionList);
        transactionsWindow.ShowAll();
    }


    private void OpenAddTransactionDialog()
    {
        Dialog dialog = new Dialog("Add Transaction", this, DialogFlags.Modal);
        dialog.SetDefaultSize(400, 300);

        VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        Entry transactionTypeEntry = new Entry { PlaceholderText = "Transaction Type" };
        Entry amountEntry = new Entry { PlaceholderText = "Amount" };

        Button saveButton = new Button("Save");
        saveButton.Clicked += (s, e) =>
        {
            string transactionType = transactionTypeEntry.Text;
            decimal amount = decimal.Parse(amountEntry.Text);

            string procedure = "EXEC ProcessOutsideTransaction @PetrolStationID, @TransactionType, @Amount, @TransactionDate";
            using (SqlCommand command = new SqlCommand(procedure, connection))
            {
                command.Parameters.AddWithValue("@PetrolStationID", 1);
                command.Parameters.AddWithValue("@TransactionType", transactionType);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@TransactionDate", DateTime.Now);

                command.ExecuteNonQuery();
            }

            dialog.Destroy();
            RefreshPetrolStationInfo();
        };

        dialogBox.PackStart(new Label("Enter Transaction Details"), false, false, 5);
        dialogBox.PackStart(transactionTypeEntry, false, false, 5);
        dialogBox.PackStart(amountEntry, false, false, 5);
        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
    }

    private void OnReceiptsClicked(object sender, EventArgs e)
    {
        OpenMessageDialog("Receipts", "Receipts functionality will go here.");
    }

    private void OnEditPetrolStationClicked(object sender, EventArgs e)
    {
        // Fetch the current data for the petrol station (assuming there's only one station, so ID = 1)
        string query = "SELECT Name, Address, ContactNumber FROM PetrolStation WHERE ID = 1";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string currentName = reader["Name"].ToString();
                    string currentAddress = reader["Address"].ToString();
                    string currentContactNumber = reader["ContactNumber"].ToString();

                    // Create the dialog window for editing petrol station data
                    Dialog editDialog = new Dialog("Edit Petrol Station", this, DialogFlags.Modal);
                    editDialog.SetDefaultSize(400, 300);

                    VBox dialogBox = new VBox();
                    editDialog.ContentArea.PackStart(dialogBox, true, true, 0);

                    // Input fields for Name, Address, and Contact Number
                    Entry nameEntry = new Entry { Text = currentName };
                    Entry addressEntry = new Entry { Text = currentAddress };
                    Entry contactNumberEntry = new Entry { Text = currentContactNumber };

                    // Add fields to the dialog
                    dialogBox.PackStart(new Label("Petrol Station Name:"), false, false, 5);
                    dialogBox.PackStart(nameEntry, false, false, 5);
                    dialogBox.PackStart(new Label("Address:"), false, false, 5);
                    dialogBox.PackStart(addressEntry, false, false, 5);
                    dialogBox.PackStart(new Label("Contact Number:"), false, false, 5);
                    dialogBox.PackStart(contactNumberEntry, false, false, 5);

                    // OK button to save the changes
                    Button okButton = new Button("OK");
                    okButton.Clicked += (sender, e) =>
                    {
                        string newName = nameEntry.Text;
                        string newAddress = addressEntry.Text;
                        string newContactNumber = contactNumberEntry.Text;

                        // Update the database with the new data
                        string updateQuery = "UPDATE PetrolStation SET Name = @Name, Address = @Address, ContactNumber = @ContactNumber WHERE ID = 1";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Name", newName);
                            updateCommand.Parameters.AddWithValue("@Address", newAddress);
                            updateCommand.Parameters.AddWithValue("@ContactNumber", newContactNumber);
                            updateCommand.ExecuteNonQuery();
                        }

                        // Close the dialog and refresh the station info
                        editDialog.Destroy();
                        RefreshPetrolStationInfo();  // Assuming you have a method to refresh the petrol station's info in the UI
                    };

                    // Cancel button to close the dialog without saving
                    Button cancelButton = new Button("Cancel");
                    cancelButton.Clicked += (sender, e) => editDialog.Destroy();

                    // Add buttons to the action area
                    editDialog.ActionArea.PackStart(okButton, false, false, 5);
                    editDialog.ActionArea.PackStart(cancelButton, false, false, 5);

                    // Show the dialog
                    editDialog.ShowAll();
                }
            }
        }
    }


    VBox receiptList = new VBox();

    private void OnReceiptManagementClicked(object sender, EventArgs e)
    {
        // Create a new dialog window for managing receipts
        Dialog receiptDialog = new Dialog("Manage Receipts", this, DialogFlags.Modal);
        receiptDialog.SetDefaultSize(600, 400);

        VBox dialogBox = new VBox();
        receiptDialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Add Receipt Button
        Button addReceiptButton = new Button("Add Receipt");
        addReceiptButton.Clicked += OnAddReceiptClicked;
        dialogBox.PackStart(addReceiptButton, false, false, 5);

        // Scrolled window for receipt list
        ScrolledWindow scroll = new ScrolledWindow();
        scroll.Add(receiptList);
        dialogBox.PackStart(scroll, true, true, 10);

        // Populate the receipt list
        PopulateReceiptList(receiptList);
        
        receiptDialog.ShowAll();
    }

    private void PopulateReceiptList(VBox receiptList)
    {
        // Clear existing receipt list items
        receiptList.Children.ToList().ForEach(widget => receiptList.Remove(widget));

        // Query the database for receipts
        string query = "SELECT ID, ReceiptDate, GasAmount, PetrolAmount, LMGAmount, TotalPrice FROM Receipt WHERE PetrolStationID = 1";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    HBox receiptRow = new HBox();

                    int id = (int)reader["ID"];
                    DateTime receiptDate = (DateTime)reader["ReceiptDate"];
                    decimal gasAmount = (decimal)reader["GasAmount"];
                    decimal petrolAmount = (decimal)reader["PetrolAmount"];
                    decimal lmgAmount = (decimal)reader["LMGAmount"];
                    decimal totalPrice = (decimal)reader["TotalPrice"];

                    Label receiptInfo = new Label($"{receiptDate.ToShortDateString()} | Gas: {gasAmount}L | Petrol: {petrolAmount}L | LMG: {lmgAmount}L | Total: ${totalPrice}");
                    Button editButton = new Button("Edit");
                    Button deleteButton = new Button("Delete");

                    editButton.Clicked += (sender, args) => ShowEditReceiptDialog(id, receiptDate, gasAmount, petrolAmount, lmgAmount, totalPrice);
                    deleteButton.Clicked += (sender, args) => DeleteReceipt(id);

                    receiptRow.PackStart(receiptInfo, true, true, 5);
                    receiptRow.PackStart(editButton, false, false, 5);
                    receiptRow.PackStart(deleteButton, false, false, 5);

                    receiptList.PackStart(receiptRow, false, false, 5);
                }
            }
        }
        RefreshPetrolStationInfo();
        // Refresh UI
        receiptList.ShowAll();
    }

    private void OnAddReceiptClicked(object sender, EventArgs args)
    {
        Dialog dialog = new Dialog("Add Receipt", this, DialogFlags.Modal);
        dialog.SetDefaultSize(400, 500);

        VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Input fields
        Label gasLabel = new Label("Gas Sold (L):");
        SpinButton gasEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label petrolLabel = new Label("Petrol Sold (L):");
        SpinButton petrolEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label lmgLabel = new Label("LMG Sold (L):");
        SpinButton lmgEntry = new SpinButton(0, 10000, 0.01) { Value = 0 };

        Label dateLabel = new Label("Transaction Date:");
        Calendar datePicker = new Calendar();

        Button saveButton = new Button("Save");
        saveButton.Clicked += (s, e) =>
        {
            decimal gasAmount = (decimal)gasEntry.Value;
            decimal petrolAmount = (decimal)petrolEntry.Value;
            decimal lmgAmount = (decimal)lmgEntry.Value;
            DateTime receiptDate = new DateTime((int)datePicker.Date.Year, (int)datePicker.Date.Month, (int)datePicker.Date.Day);

            // Ensure the price list exists for the selected date
            string priceQuery = "SELECT COUNT(*) FROM PriceList WHERE Date = @Date";
            using (SqlCommand priceCommand = new SqlCommand(priceQuery, connection))
            {
                priceCommand.Parameters.AddWithValue("@Date", receiptDate);
                int priceListCount = (int)priceCommand.ExecuteScalar();
                if (priceListCount == 0)
                {
                    MessageDialog errorDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Price list for the selected date is not available.");
                    errorDialog.Run();
                    errorDialog.Destroy();
                    return;
                }
            }

            // Call the procedure without calculating the price in C#
            string insertQuery = "EXEC SellFuel @PetrolStationID, @ReceiptDate, @GasAmount, @PetrolAmount, @LMGAmount";
            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@PetrolStationID", 1); // Example PetrolStationID, replace with dynamic value if necessary
                insertCommand.Parameters.AddWithValue("@ReceiptDate", receiptDate);
                insertCommand.Parameters.AddWithValue("@GasAmount", gasAmount);
                insertCommand.Parameters.AddWithValue("@PetrolAmount", petrolAmount);
                insertCommand.Parameters.AddWithValue("@LMGAmount", lmgAmount);
                insertCommand.ExecuteNonQuery();
            }

            dialog.Destroy();
            PopulateReceiptList(receiptList);
        };

        // Add UI elements to dialog
        dialogBox.PackStart(gasLabel, false, false, 5);
        dialogBox.PackStart(gasEntry, false, false, 5);
        dialogBox.PackStart(petrolLabel, false, false, 5);
        dialogBox.PackStart(petrolEntry, false, false, 5);
        dialogBox.PackStart(lmgLabel, false, false, 5);
        dialogBox.PackStart(lmgEntry, false, false, 5);
        dialogBox.PackStart(dateLabel, false, false, 5);
        dialogBox.PackStart(datePicker, false, false, 5);
        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
    }

    private void ShowEditReceiptDialog(int receiptId, DateTime receiptDate, decimal gasAmount, decimal petrolAmount, decimal lmgAmount, decimal totalPrice)
    {
        try
        {
            // Fetch the original receipt data from the database
            decimal originalGas = 0, originalPetrol = 0, originalLmg = 0;
            DateTime originalDate = DateTime.Now;

            string fetchQuery = "SELECT ReceiptDate, GasAmount, PetrolAmount, LMGAmount FROM Receipt WHERE ID = @ReceiptID";
            using (SqlCommand fetchCmd = new SqlCommand(fetchQuery, connection))
            {
                fetchCmd.Parameters.AddWithValue("@ReceiptID", receiptId);
                using (SqlDataReader reader = fetchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        originalDate = (DateTime)reader["ReceiptDate"];
                        originalGas = (decimal)reader["GasAmount"];
                        originalPetrol = (decimal)reader["PetrolAmount"];
                        originalLmg = (decimal)reader["LMGAmount"];
                    }
                    else
                    {
                        throw new Exception("Receipt not found.");
                    }
                }
            }

            // Fetch the current prices from the PriceList table
            decimal gasPrice = 0, petrolPrice = 0, lmgPrice = 0;
            string priceQuery = "SELECT Gas, Petrol, LMG FROM PriceList WHERE Date = @PriceDate";
            using (SqlCommand priceCmd = new SqlCommand(priceQuery, connection))
            {
                priceCmd.Parameters.AddWithValue("@PriceDate", receiptDate);
                using (SqlDataReader reader = priceCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        gasPrice = (decimal)reader["Gas"];
                        petrolPrice = (decimal)reader["Petrol"];
                        lmgPrice = (decimal)reader["LMG"];
                    }
                    else
                    {
                        throw new Exception("Price data not found for the selected date.");
                    }
                }
            }

            // Create the dialog for editing
            Dialog dialog = new Dialog("Edit Receipt", this, DialogFlags.Modal);
            dialog.SetDefaultSize(400, 500);

            VBox dialogBox = new VBox();
            dialog.ContentArea.PackStart(dialogBox, true, true, 0);

            // Input fields
            Label gasLabel = new Label("Gas Sold (L):");
            SpinButton gasEntry = new SpinButton(0, 10000, 0.01) { Value = (double)originalGas };

            Label petrolLabel = new Label("Petrol Sold (L):");
            SpinButton petrolEntry = new SpinButton(0, 10000, 0.01) { Value = (double)originalPetrol };

            Label lmgLabel = new Label("LMG Sold (L):");
            SpinButton lmgEntry = new SpinButton(0, 10000, 0.01) { Value = (double)originalLmg };

            Label dateLabel = new Label("Transaction Date:");
            Calendar datePicker = new Calendar();
            datePicker.Date = originalDate;

            Button saveButton = new Button("Save");
            saveButton.Clicked += (s, e) =>
            {
                try
                {
                    decimal newGas = (decimal)gasEntry.Value;
                    decimal newPetrol = (decimal)petrolEntry.Value;
                    decimal newLmg = (decimal)lmgEntry.Value;
                    DateTime newDate = new DateTime((int)datePicker.Date.Year, (int)datePicker.Date.Month, (int)datePicker.Date.Day);

                    // Calculate the total price
                    decimal newTotalPrice = (newGas * gasPrice) + (newPetrol * petrolPrice) + (newLmg * lmgPrice);

                    // Update receipt with the new total price
                    string updateReceiptQuery = @"
                    UPDATE Receipt
                    SET ReceiptDate = @NewDate, 
                        GasAmount = @NewGas, 
                        PetrolAmount = @NewPetrol, 
                        LMGAmount = @NewLmg, 
                        TotalPrice = @NewTotalPrice
                    WHERE ID = @ReceiptID";
                    using (SqlCommand updateReceiptCmd = new SqlCommand(updateReceiptQuery, connection))
                    {
                        updateReceiptCmd.Parameters.AddWithValue("@ReceiptID", receiptId);
                        updateReceiptCmd.Parameters.AddWithValue("@NewDate", newDate);
                        updateReceiptCmd.Parameters.AddWithValue("@NewGas", newGas);
                        updateReceiptCmd.Parameters.AddWithValue("@NewPetrol", newPetrol);
                        updateReceiptCmd.Parameters.AddWithValue("@NewLmg", newLmg);
                        updateReceiptCmd.Parameters.AddWithValue("@NewTotalPrice", newTotalPrice);
                        updateReceiptCmd.ExecuteNonQuery();
                    }

                    // Update stock and revenue
                    decimal gasDiff = newGas - originalGas;
                    decimal petrolDiff = newPetrol - originalPetrol;
                    decimal lmgDiff = newLmg - originalLmg;

                    string stockRevenueQuery = @"
                    UPDATE PetrolStation
                    SET Gas = Gas - @GasDiff,
                        Petrol = Petrol - @PetrolDiff,
                        LMG = LMG - @LmgDiff,
                        MoneyMade = MoneyMade + (@GasDiff * @GasPrice) + (@PetrolDiff * @PetrolPrice) + (@LmgDiff * @LmgPrice)
                    WHERE ID = 1"; // Assuming a single petrol station
                    using (SqlCommand stockRevenueCmd = new SqlCommand(stockRevenueQuery, connection))
                    {
                        stockRevenueCmd.Parameters.AddWithValue("@GasDiff", gasDiff);
                        stockRevenueCmd.Parameters.AddWithValue("@PetrolDiff", petrolDiff);
                        stockRevenueCmd.Parameters.AddWithValue("@LmgDiff", lmgDiff);
                        stockRevenueCmd.Parameters.AddWithValue("@GasPrice", gasPrice);
                        stockRevenueCmd.Parameters.AddWithValue("@PetrolPrice", petrolPrice);
                        stockRevenueCmd.Parameters.AddWithValue("@LmgPrice", lmgPrice);
                        stockRevenueCmd.ExecuteNonQuery();
                    }

                    dialog.Destroy();
                    PopulateReceiptList(receiptList);
                }
                catch (Exception ex)
                {
                    OpenMessageDialog("error", ex.Message);
                }
            };

            // Add UI elements to dialog
            dialogBox.PackStart(gasLabel, false, false, 5);
            dialogBox.PackStart(gasEntry, false, false, 5);
            dialogBox.PackStart(petrolLabel, false, false, 5);
            dialogBox.PackStart(petrolEntry, false, false, 5);
            dialogBox.PackStart(lmgLabel, false, false, 5);
            dialogBox.PackStart(lmgEntry, false, false, 5);
            dialogBox.PackStart(dateLabel, false, false, 5);
            dialogBox.PackStart(datePicker, false, false, 5);
            dialogBox.PackStart(saveButton, false, false, 5);

            dialog.ShowAll();
        }
        catch (Exception ex)
        {
            OpenMessageDialog("error", ex.Message);
        }
    }



    private void DeleteReceipt(int receiptId)
    {
        try
        {
            // Fetch receipt details to roll back stock and revenue
            string fetchQuery = "SELECT GasAmount, PetrolAmount, LMGAmount, PriceListDate FROM Receipt WHERE ID = @ReceiptID";
            decimal gasAmount = 0, petrolAmount = 0, lmgAmount = 0;
            DateTime priceListDate = DateTime.Now;

            using (SqlCommand fetchCmd = new SqlCommand(fetchQuery, connection))
            {
                fetchCmd.Parameters.AddWithValue("@ReceiptID", receiptId);
                using (SqlDataReader reader = fetchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        gasAmount = (decimal)reader["GasAmount"];
                        petrolAmount = (decimal)reader["PetrolAmount"];
                        lmgAmount = (decimal)reader["LMGAmount"];
                        priceListDate = (DateTime)reader["PriceListDate"];
                    }
                    else
                    {
                        throw new Exception("Receipt not found.");
                    }
                }
            }

            // Fetch the prices for the rollback calculation
            string priceQuery = "SELECT Gas, Petrol, LMG FROM PriceList WHERE Date = @PriceListDate";
            decimal gasPrice = 0, petrolPrice = 0, lmgPrice = 0;

            using (SqlCommand priceCmd = new SqlCommand(priceQuery, connection))
            {
                priceCmd.Parameters.AddWithValue("@PriceListDate", priceListDate);
                using (SqlDataReader reader = priceCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        gasPrice = (decimal)reader["Gas"];
                        petrolPrice = (decimal)reader["Petrol"];
                        lmgPrice = (decimal)reader["LMG"];
                    }
                    else
                    {
                        throw new Exception("Price data not found for the given date.");
                    }
                }
            }

            // Delete the receipt
            string deleteQuery = "DELETE FROM Receipt WHERE ID = @ReceiptID";
            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection))
            {
                deleteCmd.Parameters.AddWithValue("@ReceiptID", receiptId);
                deleteCmd.ExecuteNonQuery();
            }

            // Rollback stock and revenue
            string rollbackQuery = @"
        UPDATE PetrolStation
        SET Gas = Gas + @GasAmount,
            Petrol = Petrol + @PetrolAmount,
            LMG = LMG + @LmgAmount,
            MoneyMade = MoneyMade - (@GasAmount * @GasPrice) - (@PetrolAmount * @PetrolPrice) - (@LmgAmount * @LmgPrice)
        WHERE ID = 1"; // Assuming a single petrol station
            using (SqlCommand rollbackCmd = new SqlCommand(rollbackQuery, connection))
            {
                rollbackCmd.Parameters.AddWithValue("@GasAmount", gasAmount);
                rollbackCmd.Parameters.AddWithValue("@PetrolAmount", petrolAmount);
                rollbackCmd.Parameters.AddWithValue("@LmgAmount", lmgAmount);
                rollbackCmd.Parameters.AddWithValue("@GasPrice", gasPrice);
                rollbackCmd.Parameters.AddWithValue("@PetrolPrice", petrolPrice);
                rollbackCmd.Parameters.AddWithValue("@LmgPrice", lmgPrice);
                rollbackCmd.ExecuteNonQuery();
            }

            // Refresh the receipt list
            PopulateReceiptList(receiptList);

        }
        catch (Exception ex)
        {
            OpenMessageDialog("error", ex.Message);
        }
    }


    private void OnPriceListManagementClicked(object sender, EventArgs e)
    {
        // Check if today's price list exists
        string checkQuery = "SELECT COUNT(*) FROM PriceList WHERE Date = CAST(GETDATE() AS DATE)";
        using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
        {
            int count = (int)checkCmd.ExecuteScalar();
            if (count > 0)
            {
                MessageDialog errorDialog = new MessageDialog(this,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    "Today's price list has already been created.");
                errorDialog.Run();
                errorDialog.Destroy();
                return;
            }
        }

        // Create a dialog to input today's prices
        Dialog dialog = new Dialog("Create Today's Price List", this, DialogFlags.Modal);
        dialog.SetDefaultSize(300, 200);

        VBox dialogBox = new VBox();
        dialog.ContentArea.PackStart(dialogBox, true, true, 0);

        // Input fields
        Label gasLabel = new Label("Gas Price (per liter):");
        SpinButton gasEntry = new SpinButton(0, 1000, 0.01);

        Label petrolLabel = new Label("Petrol Price (per liter):");
        SpinButton petrolEntry = new SpinButton(0, 1000, 0.01);

        Label lmgLabel = new Label("LMG Price (per liter):");
        SpinButton lmgEntry = new SpinButton(0, 1000, 0.01);

        Button saveButton = new Button("Save");
        saveButton.Clicked += (s, ev) =>
        {
            // Retrieve entered prices
            decimal gasPrice = (decimal)gasEntry.Value;
            decimal petrolPrice = (decimal)petrolEntry.Value;
            decimal lmgPrice = (decimal)lmgEntry.Value;

            // Insert today's price list into the database
            string insertQuery = @"
            INSERT INTO PriceList (Date, Gas, Petrol, LMG)
            VALUES (CAST(GETDATE() AS DATE), @GasPrice, @PetrolPrice, @LmgPrice)";
            using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
            {
                insertCmd.Parameters.AddWithValue("@GasPrice", gasPrice);
                insertCmd.Parameters.AddWithValue("@PetrolPrice", petrolPrice);
                insertCmd.Parameters.AddWithValue("@LmgPrice", lmgPrice);
                insertCmd.ExecuteNonQuery();
            }

            MessageDialog successDialog = new MessageDialog(this,
                DialogFlags.Modal,
                MessageType.Info,
                ButtonsType.Ok,
                "Price list for today has been successfully created.");
            successDialog.Run();
            successDialog.Destroy();

            dialog.Destroy();
        };

        // Add UI elements to dialog
        dialogBox.PackStart(gasLabel, false, false, 5);
        dialogBox.PackStart(gasEntry, false, false, 5);
        dialogBox.PackStart(petrolLabel, false, false, 5);
        dialogBox.PackStart(petrolEntry, false, false, 5);
        dialogBox.PackStart(lmgLabel, false, false, 5);
        dialogBox.PackStart(lmgEntry, false, false, 5);
        dialogBox.PackStart(saveButton, false, false, 5);

        dialog.ShowAll();
    }


    private void OpenMessageDialog(string title, string message)
    {
        MessageDialog dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, message);
        dialog.Title = title;
        dialog.Run();
        dialog.Destroy();
    }

    public static void Main()
    {
        Application.Init();
        new GasStationManagementApp();
        Application.Run();
    }
}