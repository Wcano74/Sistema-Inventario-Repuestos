/*
============================================================
  SCRIPT DE DATOS DUMMY - SISTEMA DE INVENTARIO
  Ejecutar directamente en la base de datos SQL Server.
  Prerequisito: Debe existir al menos un usuario Admin
  con EmployeeCode = '00001' en AspNetUsers.
============================================================
*/

BEGIN TRANSACTION;

-- =============================================
-- 1. CATEGORIAS (10)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Repuestos de Motor')
    INSERT INTO Categories (Name, Description) VALUES (N'Repuestos de Motor', N'Pistones, anillos, juntas, válvulas y componentes internos del motor');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Frenos')
    INSERT INTO Categories (Name, Description) VALUES (N'Frenos', N'Pastillas, discos, zapatas, cilindros y líquido de frenos');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Suspensión')
    INSERT INTO Categories (Name, Description) VALUES (N'Suspensión', N'Amortiguadores, resortes, rótulas y terminales');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Filtros')
    INSERT INTO Categories (Name, Description) VALUES (N'Filtros', N'Filtros de aceite, aire, combustible y cabina');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Lubricantes')
    INSERT INTO Categories (Name, Description) VALUES (N'Lubricantes', N'Aceites de motor, transmisión, grasas y aditivos');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Eléctrico')
    INSERT INTO Categories (Name, Description) VALUES (N'Eléctrico', N'Baterías, alternadores, bujías, cables y fusibles');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Transmisión')
    INSERT INTO Categories (Name, Description) VALUES (N'Transmisión', N'Clutch, discos, volantes y aceite de transmisión');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Carrocería')
    INSERT INTO Categories (Name, Description) VALUES (N'Carrocería', N'Faros, espejos, parachoques y accesorios exteriores');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Refrigeración')
    INSERT INTO Categories (Name, Description) VALUES (N'Refrigeración', N'Radiadores, termostatos, mangueras y refrigerante');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Accesorios')
    INSERT INTO Categories (Name, Description) VALUES (N'Accesorios', N'Ambientadores, tapetes, forros y varios');

-- =============================================
-- 2. PROVEEDORES (6)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'AutoPartes Guatemala')
    INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES (N'AutoPartes Guatemala', N'Carlos Mendoza', N'2334-5566', N'ventas@autopartesgt.com');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Distribuidora Frenos Express')
    INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES (N'Distribuidora Frenos Express', N'María Fernández', N'2445-6677', N'info@frenosexp.com');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Lubricantes del Istmo')
    INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES (N'Lubricantes del Istmo', N'Roberto Juárez', N'2556-7788', N'pedidos@lubistmo.com');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Repuestos Japoneses S.A.')
    INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES (N'Repuestos Japoneses S.A.', N'Kenji Tanaka', N'2667-8899', N'contacto@repjaponeses.com');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'ElectroAuto')
    INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES (N'ElectroAuto', N'Ana Lucía Reyes', N'2778-9900', N'ventas@electroauto.com');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'ImportaMotor')
    INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES (N'ImportaMotor', N'Diego Castillo', N'2889-0011', N'diego@importamotor.com');

-- =============================================
-- 3. PRODUCTOS (40)
-- =============================================
DECLARE @CatMotor int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Repuestos de Motor');
DECLARE @CatFreno int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Frenos');
DECLARE @CatSusp int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Suspensión');
DECLARE @CatFiltro int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Filtros');
DECLARE @CatLubri int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Lubricantes');
DECLARE @CatElect int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Eléctrico');
DECLARE @CatTrans int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Transmisión');
DECLARE @CatCarr int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Carrocería');
DECLARE @CatRefri int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Refrigeración');
DECLARE @CatAcce int = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Accesorios');

DECLARE @Prov1 int = (SELECT TOP 1 Id FROM Suppliers WHERE Name = N'AutoPartes Guatemala');
DECLARE @Prov2 int = (SELECT TOP 1 Id FROM Suppliers WHERE Name = N'Distribuidora Frenos Express');
DECLARE @Prov3 int = (SELECT TOP 1 Id FROM Suppliers WHERE Name = N'Lubricantes del Istmo');
DECLARE @Prov4 int = (SELECT TOP 1 Id FROM Suppliers WHERE Name = N'Repuestos Japoneses S.A.');
DECLARE @Prov5 int = (SELECT TOP 1 Id FROM Suppliers WHERE Name = N'ElectroAuto');
DECLARE @Prov6 int = (SELECT TOP 1 Id FROM Suppliers WHERE Name = N'ImportaMotor');

-- Repuestos de Motor
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000001001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Juego de Pistones 1.6L Toyota', N'Pistones estándar para motor 1.6L Corolla 2014-2019', '7501000001001', 850.00, 520.00, 12, 3, @CatMotor, @Prov4, N'NPR', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000001002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Junta de Culata Honda Civic', N'Junta de culata multicapa 1.8L 2012-2016', '7501000001002', 380.00, 230.00, 8, 2, @CatMotor, @Prov4, N'Fel-Pro', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000001003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Kit de Anillos Nissan Sentra', N'Anillos de pistón estándar 1.8L', '7501000001003', 420.00, 260.00, 15, 4, @CatMotor, @Prov1, N'NPR', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000001004')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Válvulas de Admisión Hyundai', N'Juego 4 válvulas admisión Accent 1.4L', '7501000001004', 290.00, 175.00, 10, 3, @CatMotor, @Prov6, N'TRW', 1);

-- Frenos
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000002001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Pastillas de Freno Delanteras Toyota', N'Pastillas cerámicas Corolla/Yaris 2015+', '7501000002001', 185.00, 110.00, 30, 8, @CatFreno, @Prov2, N'Bosch', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000002002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Disco de Freno Ventilado Nissan', N'Disco ventilado delantero Sentra 2013+', '7501000002002', 320.00, 195.00, 18, 4, @CatFreno, @Prov2, N'Brembo', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000002003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Zapatas de Freno Traseras Suzuki', N'Zapatas traseras Swift/Celerio', '7501000002003', 125.00, 72.00, 22, 5, @CatFreno, @Prov2, N'FBK', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000002004')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Líquido de Frenos DOT 4 1L', N'Líquido de frenos sintético alta temperatura', '7501000002004', 65.00, 38.00, 40, 10, @CatFreno, @Prov3, N'Castrol', 1);

-- Suspensión
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000003001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Amortiguador Delantero KYB Toyota', N'Amortiguador gas KYB Excel-G Corolla', '7501000003001', 475.00, 310.00, 14, 4, @CatSusp, @Prov4, N'KYB', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000003002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Rótula Inferior Honda CR-V', N'Rótula inferior izquierda/derecha', '7501000003002', 195.00, 115.00, 10, 3, @CatSusp, @Prov1, N'555', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000003003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Terminal de Dirección Nissan', N'Terminal exterior dirección March/Versa', '7501000003003', 135.00, 78.00, 20, 5, @CatSusp, @Prov1, N'CTR', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000003004')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Resorte Espiral Trasero Hyundai', N'Par de resortes traseros Tucson 2016+', '7501000003004', 560.00, 350.00, 6, 2, @CatSusp, @Prov6, N'Lesjöfors', 1);

-- Filtros
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000004001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Filtro de Aceite Toyota Corolla', N'Filtro aceite rosca 90915-YZZD1', '7501000004001', 45.00, 25.00, 60, 15, @CatFiltro, @Prov4, N'Denso', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000004002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Filtro de Aire Honda Civic', N'Filtro aire motor 1.5T/1.8 2012+', '7501000004002', 75.00, 42.00, 35, 8, @CatFiltro, @Prov4, N'Mann Filter', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000004003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Filtro de Combustible Universal', N'Filtro combustible en línea 5/16"', '7501000004003', 35.00, 18.00, 45, 10, @CatFiltro, @Prov1, N'Wix', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000004004')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Filtro de Cabina Nissan Sentra', N'Filtro aire acondicionado antipolen', '7501000004004', 85.00, 48.00, 25, 5, @CatFiltro, @Prov1, N'Bosch', 1);

-- Lubricantes
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000005001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Aceite Motor 5W-30 Sintético 4L', N'Aceite sintético completo API SP', '7501000005001', 285.00, 180.00, 35, 10, @CatLubri, @Prov3, N'Castrol Edge', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000005002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Aceite Motor 10W-40 Semi 4L', N'Aceite semi-sintético API SN', '7501000005002', 195.00, 120.00, 40, 10, @CatLubri, @Prov3, N'Mobil Super', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000005003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Aceite Transmisión ATF 1L', N'Aceite transmisión automática Dexron VI', '7501000005003', 95.00, 58.00, 28, 8, @CatLubri, @Prov3, N'Valvoline', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000005004')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Grasa Multiuso EP2 500g', N'Grasa de litio para rodamientos', '7501000005004', 55.00, 30.00, 20, 5, @CatLubri, @Prov3, N'Shell', 1);

-- Eléctrico
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000006001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Batería 45Ah CCA 400', N'Batería libre mantenimiento 12V autos pequeños', '7501000006001', 650.00, 420.00, 10, 3, @CatElect, @Prov5, N'Bosch', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000006002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Bujías Iridium Toyota x4', N'Bujías iridium larga duración Corolla', '7501000006002', 220.00, 140.00, 18, 5, @CatElect, @Prov5, N'NGK', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000006003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Alternador Remanufacturado Honda', N'Alternador 80A Civic 2012-2016', '7501000006003', 1250.00, 780.00, 4, 2, @CatElect, @Prov5, N'Denso', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000006004')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Cables de Bujía Nissan Sentra', N'Juego cables silicona alta resistencia', '7501000006004', 175.00, 105.00, 12, 3, @CatElect, @Prov5, N'NGK', 1);

-- Transmisión
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000007001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Kit de Clutch Toyota Hilux', N'Kit completo: disco, prensa, collarín', '7501000007001', 1850.00, 1200.00, 5, 2, @CatTrans, @Prov6, N'Valeo', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000007002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Disco de Clutch Hyundai Accent', N'Disco embrague 200mm 1.4L/1.6L', '7501000007002', 450.00, 280.00, 8, 2, @CatTrans, @Prov6, N'Sachs', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000007003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Collarín de Embrague Suzuki', N'Collarín hidráulico Swift 1.2L', '7501000007003', 320.00, 195.00, 7, 2, @CatTrans, @Prov1, N'NSK', 1);

-- Carrocería
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000008001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Faro Delantero Izq. Toyota Corolla', N'Faro halógeno lado conductor 2017-2019', '7501000008001', 750.00, 480.00, 6, 2, @CatCarr, @Prov4, N'DEPO', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000008002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Espejo Lateral Der. Honda Civic', N'Espejo eléctrico con señal 2016+', '7501000008002', 520.00, 330.00, 4, 2, @CatCarr, @Prov4, N'TYC', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000008003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Stop Trasero Derecho Nissan Versa', N'Stop derecho completo 2015-2019', '7501000008003', 380.00, 240.00, 5, 2, @CatCarr, @Prov1, N'DEPO', 1);

-- Refrigeración
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000009001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Radiador Aluminio Toyota Corolla', N'Radiador completo automático 2014+', '7501000009001', 980.00, 620.00, 5, 2, @CatRefri, @Prov6, N'Koyo', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000009002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Termostato Honda Civic', N'Termostato 82°C con empaque 1.8L', '7501000009002', 120.00, 68.00, 15, 4, @CatRefri, @Prov4, N'Tama', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000009003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Refrigerante Verde 50/50 1Gal', N'Anticongelante premezclado', '7501000009003', 85.00, 48.00, 30, 8, @CatRefri, @Prov3, N'Prestone', 1);

-- Accesorios
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000010001')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Ambientador Carro Vainilla', N'Ambientador colgante larga duración', '7501000010001', 25.00, 12.00, 50, 15, @CatAcce, @Prov1, N'Little Trees', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000010002')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Tapetes Universales 4pcs', N'Juego tapetes goma negro universal', '7501000010002', 145.00, 85.00, 15, 4, @CatAcce, @Prov1, N'Motor Trend', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000010003')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Limpiaparabrisas 22" Universal', N'Escobilla limpiaparabrisas goma silicona', '7501000010003', 65.00, 35.00, 25, 6, @CatAcce, @Prov1, N'Bosch', 1);

-- Producto con stock bajo para probar alertas
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000001099')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Bomba de Agua Toyota Hilux', N'Bomba de agua con empaque 2.7L', '7501000001099', 680.00, 420.00, 1, 3, @CatMotor, @Prov6, N'GMB', 1);
IF NOT EXISTS (SELECT 1 FROM Products WHERE Barcode = '7501000002099')
    INSERT INTO Products (Name, Description, Barcode, Price, Cost, StockQuantity, MinStock, CategoryId, SupplierId, Brand, IsActive)
    VALUES (N'Cilindro Maestro Freno Nissan', N'Cilindro maestro sin ABS March', '7501000002099', 450.00, 280.00, 2, 4, @CatFreno, @Prov2, N'Aisin', 1);

-- =============================================
-- 4. CLIENTES (12)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'1234567890101')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, FiscalName, FiscalAddress, CreatedAt, IsActive)
    VALUES (N'Juan Carlos Pérez López', N'1234567890101', N'5512-3344', N'jcperez@gmail.com', N'Zona 10, Ciudad de Guatemala', N'Juan Carlos Pérez López', N'4a Avenida 12-55, Zona 10', DATEADD(day, -90, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'9876543210102')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, FiscalName, FiscalAddress, CreatedAt, IsActive)
    VALUES (N'María Elena García Morales', N'9876543210102', N'5523-4455', N'megarcia@hotmail.com', N'Zona 1, Antigua Guatemala', NULL, NULL, DATEADD(day, -75, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'4567891230103')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Taller Mecánico El Rápido', N'4567891230103', N'2234-5566', N'elrapido@gmail.com', N'Zona 7, Mixco', DATEADD(day, -60, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'7891234560104')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Roberto Méndez Cifuentes', N'7891234560104', N'5534-6677', N'rmendez@yahoo.com', N'Zona 5, Ciudad de Guatemala', DATEADD(day, -45, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'3216549870105')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Servicio Automotriz Los Primos', N'3216549870105', N'2245-6789', N'losprimos@outlook.com', N'Km 18 Carretera a El Salvador', DATEADD(day, -40, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'CF')
    INSERT INTO Customers (Name, NitDpi, Phone, CreatedAt, IsActive)
    VALUES (N'Consumidor Final', N'CF', NULL, DATEADD(day, -120, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'6549873210106')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Ana Patricia Solís', N'6549873210106', N'5545-7788', N'apsolis@gmail.com', N'Zona 11, Ciudad de Guatemala', DATEADD(day, -30, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'1597534860107')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Transportes Hernández', N'1597534860107', N'2256-7890', N'transhernandez@gmail.com', N'Zona 12, Villa Nueva', DATEADD(day, -25, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'7531594860108')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Luis Fernando Ramírez', N'7531594860108', N'5556-8899', N'lframirez@gmail.com', N'Zona 2, Quetzaltenango', DATEADD(day, -20, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'9517538520109')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Lubricentro Central', N'9517538520109', N'2267-8901', N'lubricentral@gmail.com', N'Zona 8, Ciudad de Guatemala', DATEADD(day, -15, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'3571594860110')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Carlos Andrés Monterroso', N'3571594860110', N'5567-9012', N'camonterroso@hotmail.com', N'Zona 3, Escuintla', DATEADD(day, -10, GETDATE()), 1);
IF NOT EXISTS (SELECT 1 FROM Customers WHERE NitDpi = N'1472583690111')
    INSERT INTO Customers (Name, NitDpi, Phone, Email, Address, CreatedAt, IsActive)
    VALUES (N'Taller Díaz & Hijos', N'1472583690111', N'2278-9023', N'tallerdiazhijos@gmail.com', N'Zona 6, Ciudad de Guatemala', DATEADD(day, -5, GETDATE()), 1);

-- =============================================
-- 5. CATEGORIAS DE GASTOS (6)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM ExpenseCategories WHERE Name = N'Alquiler')
    INSERT INTO ExpenseCategories (Name, Description, IsActive) VALUES (N'Alquiler', N'Pago mensual de arrendamiento del local', 1);
IF NOT EXISTS (SELECT 1 FROM ExpenseCategories WHERE Name = N'Servicios Básicos')
    INSERT INTO ExpenseCategories (Name, Description, IsActive) VALUES (N'Servicios Básicos', N'Agua, luz, teléfono e internet', 1);
IF NOT EXISTS (SELECT 1 FROM ExpenseCategories WHERE Name = N'Transporte')
    INSERT INTO ExpenseCategories (Name, Description, IsActive) VALUES (N'Transporte', N'Fletes, envíos y combustible', 1);
IF NOT EXISTS (SELECT 1 FROM ExpenseCategories WHERE Name = N'Mantenimiento')
    INSERT INTO ExpenseCategories (Name, Description, IsActive) VALUES (N'Mantenimiento', N'Reparaciones y limpieza del local', 1);
IF NOT EXISTS (SELECT 1 FROM ExpenseCategories WHERE Name = N'Papelería y Oficina')
    INSERT INTO ExpenseCategories (Name, Description, IsActive) VALUES (N'Papelería y Oficina', N'Materiales de oficina e impresiones', 1);
IF NOT EXISTS (SELECT 1 FROM ExpenseCategories WHERE Name = N'Publicidad')
    INSERT INTO ExpenseCategories (Name, Description, IsActive) VALUES (N'Publicidad', N'Redes sociales, volantes y anuncios', 1);

-- =============================================
-- 6. GASTOS (12) - Requiere un usuario admin
-- =============================================
DECLARE @AdminId nvarchar(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE EmployeeCode = '00001');

DECLARE @ECAlquiler int = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name = N'Alquiler');
DECLARE @ECServicios int = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name = N'Servicios Básicos');
DECLARE @ECTransporte int = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name = N'Transporte');
DECLARE @ECMantenimiento int = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name = N'Mantenimiento');
DECLARE @ECPapeleria int = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name = N'Papelería y Oficina');
DECLARE @ECPublicidad int = (SELECT TOP 1 Id FROM ExpenseCategories WHERE Name = N'Publicidad');

IF @AdminId IS NOT NULL
BEGIN
    -- Gastos del mes pasado y actual
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Alquiler local - Febrero 2026', 3500.00, DATEADD(day, -45, GETDATE()), N'REC-ALQ-001', @ECAlquiler, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Alquiler local - Marzo 2026', 3500.00, DATEADD(day, -15, GETDATE()), N'REC-ALQ-002', @ECAlquiler, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Energía eléctrica Febrero', 485.00, DATEADD(day, -40, GETDATE()), N'REC-LUZ-001', @ECServicios, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Agua potable Febrero', 120.00, DATEADD(day, -38, GETDATE()), N'REC-AGUA-001', @ECServicios, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Internet y teléfono Febrero', 350.00, DATEADD(day, -35, GETDATE()), N'REC-TEL-001', @ECServicios, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Flete importación repuestos japoneses', 1200.00, DATEADD(day, -28, GETDATE()), N'REC-FLETE-001', @ECTransporte, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Reparación aire acondicionado local', 850.00, DATEADD(day, -22, GETDATE()), N'REC-MANT-001', @ECMantenimiento, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Compra papel térmico POS', 180.00, DATEADD(day, -18, GETDATE()), N'REC-PAP-001', @ECPapeleria, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Publicidad Facebook Ads Marzo', 500.00, DATEADD(day, -12, GETDATE()), N'REC-PUB-001', @ECPublicidad, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Combustible entregas a domicilio', 320.00, DATEADD(day, -8, GETDATE()), N'REC-COMB-001', @ECTransporte, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Energía eléctrica Marzo', 510.00, DATEADD(day, -5, GETDATE()), N'REC-LUZ-002', @ECServicios, @AdminId);
    INSERT INTO Expenses (Description, Amount, Date, Reference, ExpenseCategoryId, UserId)
    VALUES (N'Volantes promocionales', 275.00, DATEADD(day, -3, GETDATE()), N'REC-PUB-002', @ECPublicidad, @AdminId);
END

-- =============================================
-- 7. CONFIGURACIONES DEL SISTEMA
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SystemConfigurations WHERE [Key] = 'BusinessName')
    INSERT INTO SystemConfigurations ([Key], [Value], [Description]) VALUES ('BusinessName', N'AutoRepuestos Guatemala', N'Nombre del negocio');
IF NOT EXISTS (SELECT 1 FROM SystemConfigurations WHERE [Key] = 'BusinessAddress')
    INSERT INTO SystemConfigurations ([Key], [Value], [Description]) VALUES ('BusinessAddress', N'6a Avenida 15-30 Zona 1, Ciudad de Guatemala', N'Dirección del negocio');
IF NOT EXISTS (SELECT 1 FROM SystemConfigurations WHERE [Key] = 'BusinessPhone')
    INSERT INTO SystemConfigurations ([Key], [Value], [Description]) VALUES ('BusinessPhone', N'2223-4455', N'Teléfono del negocio');
IF NOT EXISTS (SELECT 1 FROM SystemConfigurations WHERE [Key] = 'BusinessNIT')
    INSERT INTO SystemConfigurations ([Key], [Value], [Description]) VALUES ('BusinessNIT', N'12345678-9', N'NIT del negocio');
IF NOT EXISTS (SELECT 1 FROM SystemConfigurations WHERE [Key] = 'Currency')
    INSERT INTO SystemConfigurations ([Key], [Value], [Description]) VALUES ('Currency', N'Q', N'Símbolo de moneda');
IF NOT EXISTS (SELECT 1 FROM SystemConfigurations WHERE [Key] = 'TaxRate')
    INSERT INTO SystemConfigurations ([Key], [Value], [Description]) VALUES ('TaxRate', N'12', N'Porcentaje de IVA');

-- =============================================
-- 8. VENTAS (25 ventas distribuidas en 30 días)
--    Requiere usuario admin
-- =============================================
IF @AdminId IS NOT NULL
BEGIN
    -- Obtener IDs de productos por barcode
    DECLARE @ProdPistones int = (SELECT Id FROM Products WHERE Barcode = '7501000001001');
    DECLARE @ProdJunta int = (SELECT Id FROM Products WHERE Barcode = '7501000001002');
    DECLARE @ProdAnillos int = (SELECT Id FROM Products WHERE Barcode = '7501000001003');
    DECLARE @ProdPastillas int = (SELECT Id FROM Products WHERE Barcode = '7501000002001');
    DECLARE @ProdDisco int = (SELECT Id FROM Products WHERE Barcode = '7501000002002');
    DECLARE @ProdZapatas int = (SELECT Id FROM Products WHERE Barcode = '7501000002003');
    DECLARE @ProdLiqFrenos int = (SELECT Id FROM Products WHERE Barcode = '7501000002004');
    DECLARE @ProdAmortig int = (SELECT Id FROM Products WHERE Barcode = '7501000003001');
    DECLARE @ProdRotula int = (SELECT Id FROM Products WHERE Barcode = '7501000003002');
    DECLARE @ProdTerminal int = (SELECT Id FROM Products WHERE Barcode = '7501000003003');
    DECLARE @ProdFiltAceite int = (SELECT Id FROM Products WHERE Barcode = '7501000004001');
    DECLARE @ProdFiltAire int = (SELECT Id FROM Products WHERE Barcode = '7501000004002');
    DECLARE @ProdFiltComb int = (SELECT Id FROM Products WHERE Barcode = '7501000004003');
    DECLARE @ProdFiltCabina int = (SELECT Id FROM Products WHERE Barcode = '7501000004004');
    DECLARE @ProdAceite5W int = (SELECT Id FROM Products WHERE Barcode = '7501000005001');
    DECLARE @ProdAceite10W int = (SELECT Id FROM Products WHERE Barcode = '7501000005002');
    DECLARE @ProdATF int = (SELECT Id FROM Products WHERE Barcode = '7501000005003');
    DECLARE @ProdBateria int = (SELECT Id FROM Products WHERE Barcode = '7501000006001');
    DECLARE @ProdBujias int = (SELECT Id FROM Products WHERE Barcode = '7501000006002');
    DECLARE @ProdAlternador int = (SELECT Id FROM Products WHERE Barcode = '7501000006003');
    DECLARE @ProdClutch int = (SELECT Id FROM Products WHERE Barcode = '7501000007001');
    DECLARE @ProdFaro int = (SELECT Id FROM Products WHERE Barcode = '7501000008001');
    DECLARE @ProdRadiador int = (SELECT Id FROM Products WHERE Barcode = '7501000009001');
    DECLARE @ProdRefrigerante int = (SELECT Id FROM Products WHERE Barcode = '7501000009003');
    DECLARE @ProdAmbientador int = (SELECT Id FROM Products WHERE Barcode = '7501000010001');
    DECLARE @ProdLimpia int = (SELECT Id FROM Products WHERE Barcode = '7501000010003');

    -- Obtener IDs de clientes
    DECLARE @CustJuan int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'1234567890101');
    DECLARE @CustMaria int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'9876543210102');
    DECLARE @CustTaller int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'4567891230103');
    DECLARE @CustRoberto int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'7891234560104');
    DECLARE @CustPrimos int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'3216549870105');
    DECLARE @CustCF int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'CF');
    DECLARE @CustTransp int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'1597534860107');
    DECLARE @CustLubri int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'9517538520109');
    DECLARE @CustDiaz int = (SELECT TOP 1 Id FROM Customers WHERE NitDpi = N'1472583690111');

    DECLARE @SId int;

    -- === VENTA 1 - Hace 28 días: Servicio completo taller ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -28, GETDATE()), 1555.00, @AdminId, 1600.00, 45.00, 0, 1, 0, 0, @CustTaller);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPastillas, 2, 185.00, 370.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdDisco, 2, 320.00, 640.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 1, 45.00, 45.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 1, 285.00, 285.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite10W, 1, 195.00, 195.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmbientador, 1, 25.00, 25.00 - 5.00);

    -- === VENTA 2 - Hace 26 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -26, GETDATE()), 475.00, @AdminId, 500.00, 25.00, 0, 1, 0, 0, @CustJuan);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmortig, 1, 475.00, 475.00);

    -- === VENTA 3 - Hace 24 días: Aceite y filtros ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -24, GETDATE()), 405.00, @AdminId, 405.00, 0.00, 1, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 1, 285.00, 285.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 1, 45.00, 45.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAire, 1, 75.00, 75.00);

    -- === VENTA 4 - Hace 22 días: Taller grande ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -22, GETDATE()), 2300.00, @AdminId, 2300.00, 0.00, 1, 1, 0, 0, @CustPrimos);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdClutch, 1, 1850.00, 1850.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdATF, 3, 95.00, 285.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdLiqFrenos, 1, 65.00, 65.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltComb, 2, 35.00, 70.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmbientador, 1, 25.00, 30.00 - 0.00);

    -- === VENTA 5 - Hace 20 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -20, GETDATE()), 650.00, @AdminId, 700.00, 50.00, 0, 1, 0, 0, @CustRoberto);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdBateria, 1, 650.00, 650.00);

    -- === VENTA 6 - Hace 18 días: Varios items ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -18, GETDATE()), 830.00, @AdminId, 830.00, 0.00, 0, 1, 0, 0, @CustTaller);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdRotula, 2, 195.00, 390.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdTerminal, 2, 135.00, 270.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdRefrigerante, 2, 85.00, 170.00);

    -- === VENTA 7 - Hace 16 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -16, GETDATE()), 750.00, @AdminId, 750.00, 0.00, 1, 1, 0, 0, @CustMaria);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFaro, 1, 750.00, 750.00);

    -- === VENTA 8 - Hace 14 días: Lubricentro compra lote ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -14, GETDATE()), 2135.00, @AdminId, 2200.00, 65.00, 0, 1, 0, 0, @CustLubri);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 3, 285.00, 855.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite10W, 4, 195.00, 780.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 5, 45.00, 225.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAire, 3, 75.00, 225.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmbientador, 2, 25.00, 50.00);

    -- === VENTA 9 - Hace 12 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -12, GETDATE()), 380.00, @AdminId, 400.00, 20.00, 0, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdJunta, 1, 380.00, 380.00);

    -- === VENTA 10 - Hace 10 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -10, GETDATE()), 1250.00, @AdminId, 1250.00, 0.00, 1, 1, 0, 0, @CustDiaz);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAlternador, 1, 1250.00, 1250.00);

    -- === VENTA 11 - Hace 9 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -9, GETDATE()), 505.00, @AdminId, 505.00, 0.00, 0, 1, 0, 0, @CustTransp);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPastillas, 1, 185.00, 185.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdZapatas, 1, 125.00, 125.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite10W, 1, 195.00, 195.00);

    -- === VENTA 12 - Hace 8 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -8, GETDATE()), 220.00, @AdminId, 220.00, 0.00, 0, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdBujias, 1, 220.00, 220.00);

    -- === VENTA 13 - Hace 7 días: Lote grande transportes ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -7, GETDATE()), 3245.00, @AdminId, 3245.00, 0.00, 1, 1, 0, 0, @CustTransp);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPistones, 2, 850.00, 1700.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAnillos, 2, 420.00, 840.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 3, 45.00, 135.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 2, 285.00, 570.00);

    -- === VENTA 14 - Hace 6 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -6, GETDATE()), 980.00, @AdminId, 1000.00, 20.00, 0, 1, 0, 0, @CustTaller);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdRadiador, 1, 980.00, 980.00);

    -- === VENTA 15 - Hace 5 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -5, GETDATE()), 435.00, @AdminId, 435.00, 0.00, 0, 1, 0, 0, @CustJuan);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPastillas, 1, 185.00, 185.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 1, 45.00, 45.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltCabina, 1, 85.00, 85.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdRefrigerante, 1, 85.00, 85.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmbientador, 1, 25.00, 25.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdLimpia, 1, 65.00 - 55.00, 10.00);

    -- === VENTA 16 - Hace 4 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -4, GETDATE()), 570.00, @AdminId, 600.00, 30.00, 0, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 2, 285.00, 570.00);

    -- === VENTA 17 - Hace 4 días (2da del día) ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -4, GETDATE()), 310.00, @AdminId, 310.00, 0.00, 1, 1, 0, 0, @CustMaria);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdDisco, 1, 320.00 - 10.00, 310.00);

    -- === VENTA 18 - Hace 3 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -3, GETDATE()), 850.00, @AdminId, 850.00, 0.00, 0, 1, 0, 0, @CustPrimos);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPistones, 1, 850.00, 850.00);

    -- === VENTA 19 - Hace 3 días (2da del día) ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -3, GETDATE()), 155.00, @AdminId, 200.00, 45.00, 0, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 1, 45.00, 45.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltComb, 1, 35.00, 35.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAire, 1, 75.00, 75.00);

    -- === VENTA 20 - Hace 2 días ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -2, GETDATE()), 1320.00, @AdminId, 1320.00, 0.00, 1, 1, 0, 0, @CustDiaz);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmortig, 2, 475.00, 950.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPastillas, 2, 185.00, 370.00);

    -- === VENTA 21 - Hace 2 días (2da del día) ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -2, GETDATE()), 195.00, @AdminId, 200.00, 5.00, 0, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite10W, 1, 195.00, 195.00);

    -- === VENTA 22 - Ayer ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -1, GETDATE()), 1475.00, @AdminId, 1500.00, 25.00, 0, 1, 0, 0, @CustTaller);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdBateria, 1, 650.00, 650.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdBujias, 1, 220.00, 220.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 1, 285.00, 285.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 1, 45.00, 45.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdTerminal, 2, 135.00, 270.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmbientador, 1, 25.00 - 20.00, 5.00);

    -- === VENTA 23 - Ayer (2da del día) ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (DATEADD(day, -1, GETDATE()), 520.00, @AdminId, 520.00, 0.00, 1, 1, 0, 0, @CustRoberto);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFaro, 1, 520.00 - 0.00, 520.00);

    -- === VENTA 24 - Hoy ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (GETDATE(), 715.00, @AdminId, 715.00, 0.00, 0, 1, 0, 0, @CustLubri);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite5W, 1, 285.00, 285.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAceite10W, 1, 195.00, 195.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAceite, 2, 45.00, 90.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdFiltAire, 1, 75.00, 75.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdLimpia, 1, 65.00, 65.00);
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdAmbientador, 1, 25.00 - 20.00, 5.00);

    -- === VENTA 25 - Hoy (2da del día) ===
    INSERT INTO Sales (Date, Total, UserId, AmountPaid, Change, PaymentMethod, Status, HasRefunds, RefundedAmount, CustomerId)
    VALUES (GETDATE(), 370.00, @AdminId, 400.00, 30.00, 0, 1, 0, 0, @CustCF);
    SET @SId = SCOPE_IDENTITY();
    INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES (@SId, @ProdPastillas, 2, 185.00, 370.00);

    -- =============================================
    -- 9. AJUSTES DE INVENTARIO (6)
    -- =============================================
    INSERT INTO Adjustments (ProductId, Quantity, Date, Reason, UserId)
    VALUES (@ProdAmbientador, -5, DATEADD(day, -20, GETDATE()), N'Productos dañados por humedad', @AdminId);
    INSERT INTO Adjustments (ProductId, Quantity, Date, Reason, UserId)
    VALUES (@ProdFiltComb, 10, DATEADD(day, -15, GETDATE()), N'Recepción orden compra pendiente', @AdminId);
    INSERT INTO Adjustments (ProductId, Quantity, Date, Reason, UserId)
    VALUES (@ProdRefrigerante, -2, DATEADD(day, -10, GETDATE()), N'Derrame en bodega', @AdminId);
    INSERT INTO Adjustments (ProductId, Quantity, Date, Reason, UserId)
    VALUES (@ProdLiqFrenos, 15, DATEADD(day, -8, GETDATE()), N'Ingreso mercadería nueva', @AdminId);
    INSERT INTO Adjustments (ProductId, Quantity, Date, Reason, UserId)
    VALUES (@ProdAceite10W, -3, DATEADD(day, -5, GETDATE()), N'Diferencia en conteo físico', @AdminId);
    INSERT INTO Adjustments (ProductId, Quantity, Date, Reason, UserId)
    VALUES (@ProdBujias, 8, DATEADD(day, -2, GETDATE()), N'Devolución de proveedor (reemplazo)', @AdminId);

    -- =============================================
    -- 10. HISTORIAL DE PRODUCTOS (8)
    -- =============================================
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdAmbientador, @AdminId, N'Ajuste', -5, 45, N'Productos dañados por humedad', DATEADD(day, -20, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdFiltComb, @AdminId, N'Ingreso', 10, 55, N'Recepción orden compra pendiente', DATEADD(day, -15, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdPastillas, @AdminId, N'Venta', -2, 28, N'Venta #1 - Taller El Rápido', DATEADD(day, -28, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdAceite5W, @AdminId, N'Venta', -1, 34, N'Venta al mostrador', DATEADD(day, -24, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdPistones, @AdminId, N'Venta', -2, 10, N'Venta mayoreo Transportes Hernández', DATEADD(day, -7, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdBateria, @AdminId, N'Venta', -1, 9, N'Venta Roberto Méndez', DATEADD(day, -20, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdAlternador, @AdminId, N'Venta', -1, 3, N'Venta Taller Díaz & Hijos', DATEADD(day, -10, GETDATE()));
    INSERT INTO ProductHistories (ProductId, UserId, Action, QuantityChange, NewStock, Description, Date)
    VALUES (@ProdRadiador, @AdminId, N'Venta', -1, 4, N'Venta Taller El Rápido', DATEADD(day, -6, GETDATE()));

    -- =============================================
    -- 11. ORDENES DE COMPRA (4)
    -- =============================================
    -- Orden 1: Completada (Status 2)
    INSERT INTO PurchaseOrders (SupplierId, OrderDate, ExpectedDate, Status, TotalAmount, Notes, CreatedByUserId, ReceivedByUserId)
    VALUES (@Prov4, DATEADD(day, -30, GETDATE()), DATEADD(day, -23, GETDATE()), 2, 4250.00, N'Pedido mensual repuestos japoneses', @AdminId, @AdminId);
    DECLARE @PO1 int = SCOPE_IDENTITY();
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO1, @ProdPistones, 5, 520.00, 2600.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO1, @ProdFiltAceite, 20, 25.00, 500.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO1, @ProdFiltAire, 15, 42.00, 630.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO1, @ProdBujias, 10, 140.00 - 88.00, 520.00);

    -- Orden 2: Completada (Status 2)
    INSERT INTO PurchaseOrders (SupplierId, OrderDate, ExpectedDate, Status, TotalAmount, Notes, CreatedByUserId, ReceivedByUserId)
    VALUES (@Prov2, DATEADD(day, -25, GETDATE()), DATEADD(day, -20, GETDATE()), 2, 3890.00, N'Reposición stock frenos', @AdminId, @AdminId);
    DECLARE @PO2 int = SCOPE_IDENTITY();
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO2, @ProdPastillas, 15, 110.00, 1650.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO2, @ProdDisco, 5, 195.00, 975.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO2, @ProdZapatas, 10, 72.00, 720.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO2, @ProdLiqFrenos, 15, 38.00 - 1.67, 545.00);

    -- Orden 3: Pendiente (Status 0)
    INSERT INTO PurchaseOrders (SupplierId, OrderDate, ExpectedDate, Status, TotalAmount, Notes, CreatedByUserId)
    VALUES (@Prov3, DATEADD(day, -5, GETDATE()), DATEADD(day, 5, GETDATE()), 0, 3600.00, N'Pedido lubricantes mes de marzo', @AdminId);
    DECLARE @PO3 int = SCOPE_IDENTITY();
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO3, @ProdAceite5W, 10, 180.00, 1800.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO3, @ProdAceite10W, 10, 120.00, 1200.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO3, @ProdATF, 10, 58.00, 580.00);
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO3, @ProdRefrigerante, 0, 48.00 - 47.80, 0.20);

    -- Orden 4: Cancelada (Status 3)
    INSERT INTO PurchaseOrders (SupplierId, OrderDate, Status, TotalAmount, Notes, CreatedByUserId, CanceledByUserId)
    VALUES (@Prov6, DATEADD(day, -15, GETDATE()), 3, 2400.00, N'Cancelado: proveedor sin stock disponible', @AdminId, @AdminId);
    DECLARE @PO4 int = SCOPE_IDENTITY();
    INSERT INTO PurchaseOrderItems (PurchaseOrderId, ProductId, Quantity, UnitCost, TotalCost) VALUES (@PO4, @ProdClutch, 2, 1200.00, 2400.00);

END

-- =============================================
-- 12. ACTUALIZAR FECHAS ULTIMA COMPRA CLIENTES
-- =============================================
UPDATE Customers SET LastPurchaseDate = GETDATE() WHERE NitDpi = N'CF';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -5, GETDATE()) WHERE NitDpi = N'1234567890101';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -1, GETDATE()) WHERE NitDpi = N'7891234560104';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -1, GETDATE()) WHERE NitDpi = N'4567891230103';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -3, GETDATE()) WHERE NitDpi = N'3216549870105';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -7, GETDATE()) WHERE NitDpi = N'1597534860107';
UPDATE Customers SET LastPurchaseDate = GETDATE() WHERE NitDpi = N'9517538520109';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -10, GETDATE()) WHERE NitDpi = N'1472583690111';
UPDATE Customers SET LastPurchaseDate = DATEADD(day, -4, GETDATE()) WHERE NitDpi = N'9876543210102';

COMMIT;

PRINT '=============================================';
PRINT ' DATOS DUMMY INSERTADOS EXITOSAMENTE';
PRINT '=============================================';
PRINT '';
PRINT ' Resumen:';
PRINT '   - 10 Categorías';
PRINT '   - 6 Proveedores';
PRINT '   - 40 Productos (2 con stock bajo)';
PRINT '   - 12 Clientes';
PRINT '   - 6 Categorías de gastos';
PRINT '   - 12 Gastos';
PRINT '   - 6 Configuraciones del sistema';
PRINT '   - 25 Ventas con detalles (últimos 30 días)';
PRINT '   - 6 Ajustes de inventario';
PRINT '   - 8 Registros historial productos';
PRINT '   - 4 Órdenes de compra (2 completadas, 1 pendiente, 1 cancelada)';
PRINT '';
PRINT ' NOTA: Las ventas requieren un usuario Admin';
PRINT '       con EmployeeCode = 00001 en AspNetUsers.';
PRINT '=============================================';
