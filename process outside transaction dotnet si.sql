CREATE PROCEDURE ProcessOutsideTransaction
    @PetrolStationID INT,
    @TransactionType VARCHAR(100),
    @Amount DECIMAL(10,2),
    @TransactionDate DATE
AS
BEGIN
    -- Insert the outside transaction record
    INSERT INTO OutsideTransactions (PetrolStationID, TransactionType, Amount, TransactionDate)
    VALUES (@PetrolStationID, @TransactionType, @Amount, @TransactionDate);

    -- If it's an investment (either development or initial), add the amount to MoneyMade
    IF @TransactionType = 'Investment' OR @TransactionType = 'Development Investment'
    BEGIN
        UPDATE PetrolStation
        SET 
            MoneyMade = MoneyMade + @Amount
        WHERE ID = @PetrolStationID;
    END
    -- Otherwise, if it's a payment (e.g., rent, salaries), subtract the amount from MoneyMade
    ELSE
    BEGIN
        UPDATE PetrolStation
        SET 
            MoneyMade = MoneyMade - @Amount
        WHERE ID = @PetrolStationID;
    END
END;
