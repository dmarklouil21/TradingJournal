# Mechanical Trading Journal & Investment Tracker

A self-hosted, full-stack web application designed to log trades, manage Dollar Cost Averaging (DCA) campaigns, and track the performance of mechanical trading systems.

## Features

- **Comprehensive Trade Logging**: Record both 'Buy' and 'Sell' transactions with high precision for asset amounts and exact execution prices.
- **Campaign & DCA Tracking**: Group multiple entries and exits into "Campaigns" to accurately track the average cost basis and lifecycle phase (e.g., Accumulation, Markup, Distribution) of scaled-in positions.
- **Strategy Tagging**: Tag trades with specific mechanical strategies (e.g., *Opening Range Breakout (ORB)*, *RSI Oversold*, *DCA Core*) to analyze which setups yield the best win rates.
- **Live Price Integration**: Automatically fetch real-time market data for both cryptocurrencies and indices (e.g., Nasdaq) to calculate live position value and floating PnL.
- **Performance Dashboard**: Visualize portfolio allocation, total invested capital, realized vs. unrealized profit, and overall system win rate.
- **Trade Reviews**: Attach chart screenshots to individual transaction logs for post-trade review and emotional journaling.

## Tech Stack Recommendation

- **Frontend**: Vue.js
- **Backend**: ASP.NET Core (Web API) 
- **Database**: PostgreSQL
<!-- - **Storage**: AWS S3 (for chart screenshot uploads) -->

## Database Schema Overview

- **`Assets`**: Stores unique tickers (e.g., `NQ1!`, `BTC`).
- **`Strategies`**: Defines mechanical setups (e.g., `ORB`, `DCA`).
- **`Campaigns`**: Groups related transactions into a single cycle.
- **`Transactions`**: The main ledger for all Buys and Sells, linked to assets, campaigns, and strategies.
