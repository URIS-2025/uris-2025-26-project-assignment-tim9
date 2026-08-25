-- Creates one database per microservice on the shared MySQL instance.
-- Table schemas are still created by each service's own EF Core migrations
-- (see the README note in docker-compose.yml about running `dotnet ef database update`).

CREATE DATABASE IF NOT EXISTS `AuthDB`;
CREATE DATABASE IF NOT EXISTS `UserDB`;
CREATE DATABASE IF NOT EXISTS `ProjectDB`;
CREATE DATABASE IF NOT EXISTS `SprintDB`;
CREATE DATABASE IF NOT EXISTS `TimelogDB`;
CREATE DATABASE IF NOT EXISTS `workpackageservicedb`;
CREATE DATABASE IF NOT EXISTS `PaymentDB`;
CREATE DATABASE IF NOT EXISTS `NotificationServiceDB`;
CREATE DATABASE IF NOT EXISTS `IntegrationServiceDB`;
CREATE DATABASE IF NOT EXISTS `attachment_db`;
