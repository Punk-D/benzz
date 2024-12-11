CREATE PROCEDURE AddFuelSupply
    @PetrolStationID INT,
    @SupplierID INT,
    @GasQuantity DECIMAL(10,2),
    @PetrolQuantity DECIMAL(10,2),
    @LMGQuantity DECIMAL(10,2),
    @GasPrice DECIMAL(6,2),
    @PetrolPrice DECIMAL(6,2),
    @LMGPrice DECIMAL(6,2),
    @SupplementDate DATE
AS
BEGIN
    -- Add fuel to the petrol station inventory
    UPDATE PetrolStation
    SET 
        Gas = Gas + @GasQuantity,
        Petrol = Petrol + @PetrolQuantity,
        LMG = LMG + @LMGQuantity
    WHERE ID = @PetrolStationID;

    -- Insert into the Supplement table
    INSERT INTO Supplement (SupplierID, PetrolStationID, GasQuantity, PetrolQuantity, LMGQuantity, GasPrice, PetrolPrice, LMGPrice, SupplementDate)
    VALUES (@SupplierID, @PetrolStationID, @GasQuantity, @PetrolQuantity, @LMGQuantity, @GasPrice, @PetrolPrice, @LMGPrice, @SupplementDate);

    -- Update the MoneyMade (subtracting the total cost of the fuel supply)
    DECLARE @TotalCost DECIMAL(10,2);
    SET @TotalCost = (@GasQuantity * @GasPrice) + (@PetrolQuantity * @PetrolPrice) + (@LMGQuantity * @LMGPrice);

    UPDATE PetrolStation
    SET 
        MoneyMade = MoneyMade - @TotalCost
    WHERE ID = @PetrolStationID;
END;
