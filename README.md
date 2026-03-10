# stock-pro
CMPE-232 Project

---

# Stock_Pro Database System

This is a "Professional Inventory Management" Web project that based on Blazor web app, MSSQL Database and EF6. This project purpose is basically give information about stock levels, suppliers, and product movements. In this website you can view and edit Product and Supplier details. In the dashboard view you can see product SKU, unit types, categories, and associated suppliers. You can see warehouses and the stock items stored within them. You can track inbound and outbound stock movements and see which users authorized them. Also you can see total Product, Supplier and Warehouse count. In Admin page, you can create, read, update and delete all of these elements. Additionally, the system uses triggers to automatically update the `Updated_at` timestamp for suppliers and adjust stock quantities when a movement occurs. In the Stock Statistics page, you can view the current stock levels across different warehouses.

Implementation Steps:

## Usage

1- Create a database in MSSQL Server Management Studio with query that given below.

```sql
-- 0. DATABASE CREATION
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'stock_pro')
BEGIN
    CREATE DATABASE stock_pro;
END
GO

USE stock_pro;
GO

-- 1. SUPPLIER TABLE
CREATE TABLE [supplier] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Company_name] NVARCHAR(255) NOT NULL UNIQUE,
    [Contact_person] NVARCHAR(150) NULL,
    [Email] NVARCHAR(150) NULL,
    [Phone] NVARCHAR(50) NULL,
    [Updated_at] DATETIME DEFAULT GETDATE()
);
GO

-- Trigger for Supplier Updates
CREATE TRIGGER trg_supplier_updated
ON [supplier]
AFTER UPDATE
AS
BEGIN
    UPDATE [supplier]
    SET Updated_at = GETDATE()
    FROM [supplier] s
    INNER JOIN inserted i ON s.Id = i.Id;      
END;
GO

-- 2. PRODUCT TABLE
CREATE TABLE [product] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Name] NVARCHAR(255) NOT NULL,
    [Sku] NVARCHAR(50) NULL UNIQUE,
    [Unit_type] NVARCHAR(50) NULL,
    [Category] NVARCHAR(100) NULL,
    [Supplier_id] INT NULL,
    CONSTRAINT fk_product_supplier FOREIGN KEY ([Supplier_id]) REFERENCES [supplier]([Id]) ON DELETE SET NULL
);
GO

-- 3. WAREHOUSE TABLE
CREATE TABLE [warehouse] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Name] NVARCHAR(100) NOT NULL UNIQUE,
    [Location] NVARCHAR(255) NULL
);
GO

-- 4. STOCK_ITEM TABLE
CREATE TABLE [stock_item] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Product_id] INT NOT NULL,
    [Warehouse_id] INT NOT NULL,
    [Quantity] INT DEFAULT 0 CHECK ([Quantity] >= 0),
    [Last_purchase_price] DECIMAL(10,2) NULL,
    CONSTRAINT UK_Stock_Item UNIQUE ([Product_id], [Warehouse_id]),
    CONSTRAINT fk_stockitem_product FOREIGN KEY ([Product_id]) REFERENCES [product]([Id]) ON DELETE CASCADE,
    CONSTRAINT fk_stockitem_warehouse FOREIGN KEY ([Warehouse_id]) REFERENCES [warehouse]([Id]) ON DELETE CASCADE
);
GO

-- 5. USER TABLE
CREATE TABLE [user] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Name] NVARCHAR(100) NOT NULL,
    [Surname] NVARCHAR(100) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL,
    [Email] NVARCHAR(150) NOT NULL UNIQUE,
    [Password_hash] NVARCHAR(255) NOT NULL,
    [Created_at] DATETIME DEFAULT GETDATE()
);
GO

-- 6. STOCK_MOVEMENT TABLE
CREATE TABLE [stock_movement] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Movement_type] NVARCHAR(20) NOT NULL CHECK ([Movement_type] IN ('Inbound','Outbound','Transfer')),
    [Movement_date] DATETIME DEFAULT GETDATE(),
    [Quantity] INT NOT NULL,
    [Product_id] INT NOT NULL,
    [Warehouse_id] INT NOT NULL,
    [User_id] INT NOT NULL,
    CONSTRAINT fk_movement_product FOREIGN KEY ([Product_id]) REFERENCES [product]([Id]),
    CONSTRAINT fk_movement_warehouse FOREIGN KEY ([Warehouse_id]) REFERENCES [warehouse]([Id]),
    CONSTRAINT fk_movement_user FOREIGN KEY ([User_id]) REFERENCES [user]([Id])
);
GO

CREATE INDEX idx_movement_date ON [stock_movement]([Movement_date]);
GO

-- Trigger for Stock Movement
CREATE TRIGGER trg_update_stock_quantity
ON [stock_movement]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Handle Inbound (Add stock)
    UPDATE s
    SET s.Quantity = s.Quantity + i.Quantity
    FROM [stock_item] s
    INNER JOIN inserted i ON s.Product_id = i.Product_id AND s.Warehouse_id = i.Warehouse_id
    WHERE i.Movement_type = 'Inbound';

    -- Handle Outbound (Subtract stock)
    UPDATE s
    SET s.Quantity = s.Quantity - i.Quantity
    FROM [stock_item] s
    INNER JOIN inserted i ON s.Product_id = i.Product_id AND s.Warehouse_id = i.Warehouse_id
    WHERE i.Movement_type = 'Outbound';
END;
GO

-- 7. REVIEW TABLE (Including updates)
CREATE TABLE [review] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,       
    [Rating] INT NULL CHECK ([Rating] BETWEEN 1 AND 5),
    [Product_id] INT NOT NULL,
    [Comment] NVARCHAR(MAX) NULL,
    [User_id] INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_review_product FOREIGN KEY ([Product_id]) REFERENCES [product]([Id]) ON DELETE CASCADE
);
GO

-- 8. VIEW
CREATE VIEW vw_current_stock AS
SELECT 
    w.Name AS Warehouse, 
    p.Name AS Product, 
    si.Quantity AS Quantity 
FROM stock_item si 
JOIN warehouse w ON si.Warehouse_id = w.Id    
JOIN product p ON si.Product_id = p.Id;
GO

```

2- Open a new query in your MSSQL Server Management Studio and insert the data given below.

```sql
INSERT INTO [supplier] ([Company_name], [Contact_person], [Email], [Phone]) 
VALUES ('Tekno Toptan A.S.', 'Mehmet Demir', 'info@teknotoptan.com', '05551112233');

INSERT INTO [product] ([Name], [Sku], [Unit_type], [Category], [Supplier_id]) 
VALUES ('Gaming Laptop', 'LPT-001', 'Adet', 'Elektronik', 1);

INSERT INTO [warehouse] ([Name], [Location]) 
VALUES ('Merkez Depo', 'Ankara/Kolej');

INSERT INTO [stock_item] ([Product_id], [Warehouse_id], [Quantity], [Last_purchase_price]) 
VALUES (1, 1, 10, 25000.00);

INSERT INTO [user] ([Name], [Surname], [Role], [Email], [Password_hash]) 
VALUES ('Ahmet', 'Yilmaz', 'Operator', 'ahmet@tedu.edu.tr', 'sifre123');

-- Note: This insert will trigger trg_update_stock_quantity and update stock_item
INSERT INTO [stock_movement] ([Movement_type], [Quantity], [Product_id], [Warehouse_id], [User_id]) 
VALUES ('Inbound', 10, 1, 1, 1);

```

3- Open StockPro.sln file in StockPro folder with Visual Studio “Open a project or solution” option.

4- Change your connection string in the appsettings.json file with respect to your database’s connection string. You can access your connection string with the steps given below:
I- Click view on the top navigation bar, click server explorer.
II- Right click on Data Connection and click add a new connection and select the stock_pro database in your server.
III- Click properties on your stock_pro database.
IV- You can see your connection string in the properties window.

5- After completing all these steps, click run https button in Visual Studio. You have all the CRUD permissions in the admin page.

6- Note: Make sure you have Radzen installed in your NuGet packet manager.

7-  Do not update model from database as we fixed  recursion depth errors by some modifications to .cs files. When you update the model from databse, our modifications are reseted.

## Little Note

This project is solely for the purpose of education. We want future computer engineering students to take inspiration from our project.
