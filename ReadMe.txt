You'll need to have cTrader setup properly with .NET6.

Using the AI Algo Work - Strat 8 code, you can create a cBot which posts a HTTP Post.

Ahead of this, you'll need the appStrat8.py running, this is a Flask Server that receives the HTTP Post from the cBot and sends it to the .h5 model.

The .h5 model is made using the 2DArrayNN and the BinaryDataBuy.csv which holds historical data from the stock market, which was gathered using Data Gathering c# file ran as a backtest through cTrader.