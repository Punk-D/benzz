CREATE PROCEDURE SellFuel
    @PetrolStationID INT,
    @ReceiptDate DATE,
    @GasAmount DECIMAL(10,2),
    @PetrolAmount DECIMAL(10,2),
    @LMGAmount DECIMAL(10,2)
AS
BEGIN
    DECLARE @GasPrice DECIMAL(6,2);
    DECLARE @PetrolPrice DECIMAL(6,2);
    DECLARE @LMGPrice DECIMAL(6,2);

    -- Get the prices for the fuel types from the price list
    SELECT @GasPrice = Gas, @PetrolPrice = Petrol, @LMGPrice = LMG
    FROM PriceList
    WHERE Date = @ReceiptDate;

    -- Subtract the fuel sold from the petrol station inventory
    UPDATE PetrolStation
    SET 
        Gas = Gas - @GasAmount,
        Petrol = Petrol - @PetrolAmount,
        LMG = LMG - @LMGAmount
    WHERE ID = @PetrolStationID;

    -- Calculate the total price of the sale
    DECLARE @TotalPrice DECIMAL(10,2);
    SET @TotalPrice = (@GasAmount * @GasPrice) + (@PetrolAmount * @PetrolPrice) + (@LMGAmount * @LMGPrice);

    -- Insert the sale receipt
    INSERT INTO Receipt (PetrolStationID, ReceiptDate, GasAmount, PetrolAmount, LMGAmount, TotalPrice, PriceListDate)
    VALUES (@PetrolStationID, @ReceiptDate, @GasAmount, @PetrolAmount, @LMGAmount, @TotalPrice, @ReceiptDate);

    -- Update the MoneyMade (adding the total sale amount)
    UPDATE PetrolStation
    SET 
        MoneyMade = MoneyMade + @TotalPrice
    WHERE ID = @PetrolStationID;
END;
