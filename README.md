# Dual-Engine Trading Journal & Investment Tracker

A self-hosted, full-stack web application designed with a bifurcated architecture to completely separate long-term asset accumulation from active, mechanical trading. 

To ensure clean data and accurate performance metrics, this application is split into two isolated modules:

*   **The Investing Tracker (DCA Focus):** Built specifically for long-term portfolio growth. It logs asset purchases and groups continuous buys into Dollar Cost Averaging (DCA) campaigns to automatically calculate the true average cost basis. This module tracks the lifecycle phase of investments (e.g., Accumulation, Markup) and monitors overall holdings and unrealized profits using real-time price API integrations.
*   **The Active Trading Journal:** A dedicated, isolated environment for logging strict mechanical trading setups (such as Opening Range Breakouts or RSI conditions) on volatile instruments like indices and cryptocurrencies. It strictly measures realized PnL and system win rates by strategy. It also supports chart screenshot attachments for rigorous post-trade review, ensuring that the slow, steady data of long-term holdings never pollutes the performance metrics of your active systems.

## Features

### Module A: The Investing Tracker
*   **Investment Logging:** Record individual asset purchases (asset name, date, price, amount, and fees).
*   **DCA Campaign Grouping:** Link continuous buys of the same asset into a single "Campaign" to calculate the true average cost basis automatically.
*   **Holdings Dashboard:** A high-level list displaying total invested capital, total holdings, average cost basis, live current price, current overall value, and unrealized position profit (%).
*   **Phase Tracking:** Track the macro stage of the asset (Accumulation, Markup, Distribution).

### Module B: The Active Trading Journal
*   **Isolated Trade Entries:** Log distinct, completed transactions (Buy/Sell, exact entry/exit prices, dates, and fees).
*   **Strategy & Trigger Tagging:** Categorize the specific technical reason for the entry (e.g., ORB, RSI Oversold).
*   **Realized PnL & Win Rate Metrics:** Measure locked-in profits, losses, and overall system win rate across different strategies, entirely separated from long-term holdings.
*   **Chart Attachments:** Upload screenshots of the exact chart conditions at the moment of entry or exit for post-trade review.

### Global System Features
*   **Live Price Integration:** Backend connections to live market data APIs (e.g., Binance, Alpha Vantage) to keep the current price and floating values updated in real-time across both modules.

## Tech Stack
*   **Frontend:** Vue.js
*   **Backend:** ASP.NET Core (Web API)
*   **Database:** PostgreSQL
*   **Storage:** AWS S3 (for chart screenshot uploads)
