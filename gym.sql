-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Gép: 127.0.0.1
-- Létrehozás ideje: 2026. Ápr 27. 11:59
-- Kiszolgáló verziója: 10.4.32-MariaDB
-- PHP verzió: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Adatbázis: `gym`
--

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `cities`
--

CREATE TABLE `cities` (
  `Id` int(11) NOT NULL,
  `ZipCode` int(11) NOT NULL,
  `Name` longtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `cities`
--

INSERT INTO `cities` (`Id`, `ZipCode`, `Name`) VALUES
(1, 9700, 'Szombathely');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `gymhasproducts`
--

CREATE TABLE `gymhasproducts` (
  `Id` int(11) NOT NULL,
  `GymId` int(11) NOT NULL,
  `ProductId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `gymhasproducts`
--

INSERT INTO `gymhasproducts` (`Id`, `GymId`, `ProductId`) VALUES
(1, 1, 1),
(2, 1, 2),
(3, 1, 3),
(4, 2, 1),
(5, 2, 2),
(6, 2, 3),
(7, 3, 1),
(8, 3, 2),
(9, 3, 3),
(10, 4, 1),
(11, 4, 2),
(12, 4, 3),
(13, 5, 1),
(14, 5, 2),
(15, 5, 3),
(16, 6, 1),
(17, 6, 2),
(18, 6, 3),
(19, 7, 1),
(20, 7, 2),
(21, 7, 3),
(22, 8, 1),
(23, 8, 2),
(24, 8, 3),
(25, 9, 1),
(26, 9, 2),
(27, 9, 3);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `gyms`
--

CREATE TABLE `gyms` (
  `Id` int(11) NOT NULL,
  `Name` longtext NOT NULL,
  `Phone` longtext DEFAULT NULL,
  `Email` longtext DEFAULT NULL,
  `CityId` int(11) DEFAULT NULL,
  `OpenAt` longtext NOT NULL,
  `ImagePath` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `gyms`
--

INSERT INTO `gyms` (`Id`, `Name`, `Phone`, `Email`, `CityId`, `OpenAt`, `ImagePath`) VALUES
(1, 'Aero Fitness', '94314444', 'info@aero-fitness.hu', 1, 'H-P: 06:00-21:00', 'aero.jpg'),
(2, 'Öntöde Sportcentrum', '94500055', 'info@ontodesport.hu', 1, 'H-P: 06:00-22:00', 'ontode.jpg'),
(3, 'Iron Samson Gym', '30222333', 'iron@ironsamsongym.hu', 1, 'H-P: 06:00-22:00', 'samson.jpg'),
(4, 'Sylver Fitnesz', '94345678', 'sylver@sylver-fitnesz.hu', 1, 'H-P: 06:00-21:00', 'sylver.jpg'),
(5, 'Best Fitness', '94123456', 'info@best-fitness.hu', 1, 'H-P: 06:00-22:00', 'best.jpg'),
(6, 'MoveFit Szombathely', '94900290', 'info@movefit.hu', 1, 'H-P: 06:00-22:00, Szo-Vas: 08:00-20:00', 'movefit.jpg'),
(7, 'Fit4Lady', '30777665', 'info@fit4lady.hu', 1, 'H-P: 07:00-20:00', 'lady.jpg'),
(8, 'Gymtronic 9700', '94311411', 'info@gymtronic9700.hu', 1, 'H-P: 06:00-22:00', 'gymtronic.jpg'),
(9, 'ProGym Szombathely', '94310310', 'info@progym.hu', 1, 'H-P: 06:00-21:00', 'progym.jpg');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `products`
--

CREATE TABLE `products` (
  `Id` int(11) NOT NULL,
  `Designation` longtext NOT NULL,
  `IsTicket` tinyint(1) NOT NULL,
  `IsRental` tinyint(1) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Price` double NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `products`
--

INSERT INTO `products` (`Id`, `Designation`, `IsTicket`, `IsRental`, `IsActive`, `Price`) VALUES
(1, 'Napijegy', 1, 0, 1, 2500),
(2, 'Hetijegy', 1, 0, 1, 7000),
(3, 'Havi bérlet', 0, 0, 1, 15000);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `purchases`
--

CREATE TABLE `purchases` (
  `Id` char(36) NOT NULL,
  `IsCash` tinyint(1) NOT NULL,
  `IsCreditCard` tinyint(1) NOT NULL,
  `Amount` double NOT NULL,
  `ProductId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `CashierUserId` int(11) NOT NULL,
  `ExpirationDate` datetime(6) DEFAULT NULL,
  `GymId` int(11) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `purchases`
--

INSERT INTO `purchases` (`Id`, `IsCash`, `IsCreditCard`, `Amount`, `ProductId`, `UserId`, `CashierUserId`, `ExpirationDate`, `GymId`) VALUES
('6c48171f-1280-4f27-ab4e-fdeb352c7cdc', 1, 0, 2500, 1, 1, 1, '2026-04-18 13:47:51.104000', 2),
('ccfd2d6c-4886-41ce-b66e-fa889acf161b', 1, 0, 30000, 3, 2, 2, '2026-07-18 13:53:27.810000', 4),
('ea0b6dd1-aadd-4374-9fb6-38f4b241d09a', 1, 0, 12000, 2, 1, 1, '2026-05-18 13:48:11.695000', 5);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `ratings`
--

CREATE TABLE `ratings` (
  `Id` int(11) NOT NULL,
  `Stars` int(11) NOT NULL,
  `Message` longtext DEFAULT NULL,
  `UserId` int(11) NOT NULL,
  `GymId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `ratings`
--

INSERT INTO `ratings` (`Id`, `Stars`, `Message`, `UserId`, `GymId`) VALUES
(1, 5, 'Tökéletes!', 1, 2);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `users`
--

CREATE TABLE `users` (
  `Id` int(11) NOT NULL,
  `Name` longtext NOT NULL,
  `Email` longtext NOT NULL,
  `Password` longtext NOT NULL,
  `Role` longtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `users`
--

INSERT INTO `users` (`Id`, `Name`, `Email`, `Password`, `Role`) VALUES
(1, 'Bujtor Bálint', 'balint200509@gmail.com', 'AQAAAAIAAYagAAAAEDvCsaBe/4ZqOZOhmI+B19g/3qQ1sJe8qgilbWhTsb3H2LUECPrOudyD/CXztzNELQ==', 'user'),
(2, 'admin', 'admin@admin.hu', 'AQAAAAIAAYagAAAAEOz4Q7YSkJSGBD0R/3WNPnKbSX9/gFA99WGsPt4GRDjqtiPbJ9XPcT8zx3ttxb7fXA==', 'admin');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `__efmigrationshistory`
--

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
('20260417202123_InitialCreate', '10.0.6'),
('20260418005318_SecondCreates', '10.0.6'),
('20260418014629_ThirdCreates', '10.0.6');

--
-- Indexek a kiírt táblákhoz
--

--
-- A tábla indexei `cities`
--
ALTER TABLE `cities`
  ADD PRIMARY KEY (`Id`);

--
-- A tábla indexei `gymhasproducts`
--
ALTER TABLE `gymhasproducts`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_GymHasProducts_GymId` (`GymId`),
  ADD KEY `IX_GymHasProducts_ProductId` (`ProductId`);

--
-- A tábla indexei `gyms`
--
ALTER TABLE `gyms`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Gyms_CityId` (`CityId`);

--
-- A tábla indexei `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`Id`);

--
-- A tábla indexei `purchases`
--
ALTER TABLE `purchases`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Purchases_CashierUserId` (`CashierUserId`),
  ADD KEY `IX_Purchases_ProductId` (`ProductId`),
  ADD KEY `IX_Purchases_UserId` (`UserId`),
  ADD KEY `FK_Purchases_Gyms_GymId` (`GymId`);

--
-- A tábla indexei `ratings`
--
ALTER TABLE `ratings`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Ratings_GymId` (`GymId`),
  ADD KEY `IX_Ratings_UserId` (`UserId`);

--
-- A tábla indexei `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`);

--
-- A tábla indexei `__efmigrationshistory`
--
ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- A kiírt táblák AUTO_INCREMENT értéke
--

--
-- AUTO_INCREMENT a táblához `cities`
--
ALTER TABLE `cities`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT a táblához `gymhasproducts`
--
ALTER TABLE `gymhasproducts`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=28;

--
-- AUTO_INCREMENT a táblához `gyms`
--
ALTER TABLE `gyms`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT a táblához `products`
--
ALTER TABLE `products`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT a táblához `ratings`
--
ALTER TABLE `ratings`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT a táblához `users`
--
ALTER TABLE `users`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Megkötések a kiírt táblákhoz
--

--
-- Megkötések a táblához `gymhasproducts`
--
ALTER TABLE `gymhasproducts`
  ADD CONSTRAINT `FK_GymHasProducts_Gyms_GymId` FOREIGN KEY (`GymId`) REFERENCES `gyms` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_GymHasProducts_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE;

--
-- Megkötések a táblához `gyms`
--
ALTER TABLE `gyms`
  ADD CONSTRAINT `FK_Gyms_Cities_CityId` FOREIGN KEY (`CityId`) REFERENCES `cities` (`Id`);

--
-- Megkötések a táblához `purchases`
--
ALTER TABLE `purchases`
  ADD CONSTRAINT `FK_Purchases_Gyms_GymId` FOREIGN KEY (`GymId`) REFERENCES `gyms` (`Id`),
  ADD CONSTRAINT `FK_Purchases_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_Purchases_Users_CashierUserId` FOREIGN KEY (`CashierUserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_Purchases_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Megkötések a táblához `ratings`
--
ALTER TABLE `ratings`
  ADD CONSTRAINT `FK_Ratings_Gyms_GymId` FOREIGN KEY (`GymId`) REFERENCES `gyms` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_Ratings_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
