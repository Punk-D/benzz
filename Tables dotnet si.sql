-- Table for storing information about the petrol station
CREATE TABLE PetrolStation
(
    ID INT PRIMARY KEY IDENTITY,  -- Unique ID for the station
    Name VARCHAR(255),            -- Name of the petrol station
    Address VARCHAR(500),         -- Address of the station
    ContactNumber VARCHAR(20),    -- Contact number
    Gas DECIMAL(10,2),            -- Available quantity of gas
    Petrol DECIMAL(10,2),         -- Available quantity of petrol
    LMG DECIMAL(10,2),            -- Available quantity of LMG
    MoneyMade DECIMAL(10,2)       -- Tracks the total money made/lost by the station
);

-- Table for storing supplier information
CREATE TABLE Supplier
(
    ID INT PRIMARY KEY IDENTITY,   -- Unique ID for the supplier
    Name VARCHAR(255),              -- Name of the supplier
    Address VARCHAR(500),           -- Address of the supplier
    ContactNumber VARCHAR(20),      -- Contact number
    Email VARCHAR(100)              -- Email of the supplier
);

-- Table for storing supplement records (gas supplies)
CREATE TABLE Supplement
(
    ID INT PRIMARY KEY IDENTITY,    -- Unique ID for the supplement
    SupplierID INT,                 -- Foreign key to Supplier table
    PetrolStationID INT,            -- Foreign key to PetrolStation table
    GasQuantity DECIMAL(10,2),      -- Quantity of gas supplied
    PetrolQuantity DECIMAL(10,2),   -- Quantity of petrol supplied
    LMGQuantity DECIMAL(10,2),      -- Quantity of LMG supplied
    GasPrice DECIMAL(6,2),          -- Price at which gas was supplied
    PetrolPrice DECIMAL(6,2),       -- Price at which petrol was supplied
    LMGPrice DECIMAL(6,2),          -- Price at which LMG was supplied
    SupplementDate DATE,            -- Date of the supply
    FOREIGN KEY (SupplierID) REFERENCES Supplier(ID), -- Foreign key relationship to Supplier
    FOREIGN KEY (PetrolStationID) REFERENCES PetrolStation(ID) -- Foreign key relationship to PetrolStation
);

-- Table for storing the price list for different fuel types
CREATE TABLE PriceList
(
    Date DATE PRIMARY KEY,           -- Date when the price list was effective
    Gas DECIMAL(6,2),                -- Price of gas
    Petrol DECIMAL(6,2),             -- Price of petrol
    LMG DECIMAL(6,2)                 -- Price of LMG
);

-- Table for storing receipt information (sales transactions)
CREATE TABLE Receipt
(
    ID INT PRIMARY KEY IDENTITY,    -- Unique ID for the receipt
    PetrolStationID INT,            -- Foreign key to PetrolStation table
    ReceiptDate DATE,                -- Date when the transaction took place
    GasAmount DECIMAL(10,2),         -- Amount of gas purchased
    PetrolAmount DECIMAL(10,2),      -- Amount of petrol purchased
    LMGAmount DECIMAL(10,2),         -- Amount of LMG purchased
    TotalPrice DECIMAL(10,2),        -- Total price of the transaction
    PriceListDate DATE,              -- Foreign key to the price list (date)
    FOREIGN KEY (PetrolStationID) REFERENCES PetrolStation(ID), -- Foreign key relationship to PetrolStation
    FOREIGN KEY (PriceListDate) REFERENCES PriceList(Date) -- Foreign key relationship to PriceList
);

-- Table for storing outside transactions (payments to workers, rent, taxes, etc.)
CREATE TABLE OutsideTransactions
(
    ID INT PRIMARY KEY IDENTITY,    -- Unique ID for the outside transaction
    PetrolStationID INT,            -- Foreign key to PetrolStation table
    TransactionType VARCHAR(100),    -- Type of transaction (e.g., "Payment to Worker", "Rent", "Tax", etc.)
    Amount DECIMAL(10,2),            -- Amount of the transaction
    TransactionDate DATE,            -- Date of the transaction
    FOREIGN KEY (PetrolStationID) REFERENCES PetrolStation(ID) -- Foreign key relationship to PetrolStation
);
