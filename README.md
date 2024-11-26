This README provides details about controllers designed to fetch data for different quarters and financial years. These controllers enable seamless data retrieval by pairing years and utilizing parameters like Input, Sales, Demand, and Collection. They are particularly helpful when creating projects for organizations that require structured and parameterized data access.

Controllers Overview
1. Annual Data Controller
Purpose: Fetches data aggregated over a complete financial year.
Usage: This controller provides annual data by pairing financial years. It supports different organizational regions and ensures data is grouped by finyear_report.
Parameters Supported:
Input: Total energy input.
Sales: Total sales figures.
Demand: Energy demand.
Collection: Total collections.
2. Quarterly Data Controller
Purpose: Fetches data for specific quarters within a financial year.
Usage: Useful for more granular data analysis. Users can select the quarter (Q1, Q2, Q3, Q4) and pair it with the financial year.
Parameters Supported:
Input: Total energy input.
Sales: Quarterly sales figures.
Demand: Energy demand for the quarter.
Collection: Collections made during the quarter.
Key Features
Dynamic Data Pairing:

Supports pairing of financial years (e.g., FY 2022-2023) and quarters.
Automatically filters data based on the selected year and/or quarter.
Flexible Parameter Selection:

Choose specific parameters (Input, Sales, Demand, Collection) to fetch data for targeted analysis.
Region-Specific Queries:

Enables region-based data retrieval by using predefined region codes (e.g., MPCZ, MPEZ, MPWZ).
Regions are identified via the region column in the database.
Database Integration
Relevant Columns
The controllers fetch data using the following database columns:

finyear_report: Stores financial years (e.g., FY 2023-2024).
region: Identifies regions (e.g., Central Discom, East Discom, West Discom).
Parameters:
Total Input: Represents total energy input.
Total Sales: Reflects total sales numbers.
Total Demand: Indicates the demand for the financial period.
Total Collections: Represents collections made.
Usage Guide
Annual Data Retrieval
To fetch annual data:

Specify the financial year using the finyear_report column.
Select parameters (Input, Sales, Demand, Collection) to customize the output.
Example API Request:

json
Copy code
GET /api/annualData?year=2023-2024&region=MPCZ&parameter=Input
