-- PostgreSQL script for creating databases
-- Note: In docker-compose setup, both services use the same 'shopping' database
-- This script is for manual setup if needed

CREATE DATABASE "ShoppingOrders";
CREATE DATABASE "ShoppingPayments";

-- Grant permissions (if needed for specific user)
-- GRANT ALL PRIVILEGES ON DATABASE "ShoppingOrders" TO postgres;
-- GRANT ALL PRIVILEGES ON DATABASE "ShoppingPayments" TO postgres; 