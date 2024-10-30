using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ChatGPTAIBotFlask : Robot
    {
        private ExponentialMovingAverage _ema26;
        private ExponentialMovingAverage _ema50;
        private ExponentialMovingAverage _ema200;
        private RelativeStrengthIndex _rsi;
        
        private ExponentialMovingAverage _ema26B;
        private ExponentialMovingAverage _ema50B;
        private ExponentialMovingAverage _ema200B;
        private RelativeStrengthIndex _rsiB;

        [Parameter("Label", DefaultValue = "AIBot")]
        public String Label { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Take Profit (Pts)", DefaultValue = 3)]
        public double TakeProfitPercent { get; set; }
        
        [Parameter("Stop Loss % x 10000", DefaultValue = 10, MaxValue = 150)]
        public int StopLossPercent { get; set; }

        [Parameter("Volume Percent", DefaultValue = 1, MinValue = 0)]
        public double VolumePercent { get; set; }
        
        [Parameter("EMA Period", DefaultValue = 26, MinValue = 14, MaxValue = 200)]
        public int Period { get; set; }
        
        [Parameter("Min Level", DefaultValue = 3, MinValue = 0, MaxValue = 50)]
        public int MinLevel { get; set; }
        
       

        //Prediction Global Variables
        public int Day1;
        public int Day2;
        public int Day3;
        public int Day4;
        public int Day5;
        public int Day6;
        public int prediction;
        
        //Strategy Global Variables
        public double volumne;
        public bool TradeDone;
        public bool DayTrade;
        public double StartBalance;
        public bool TargetMet;
        

        protected override void OnStart()
        {
            var TimeFrameDaily = MarketData.GetBars(TimeFrame.Daily);
            _ema26 = Indicators.ExponentialMovingAverage(TimeFrameDaily.OpenPrices, 26);
            _ema50 = Indicators.ExponentialMovingAverage(TimeFrameDaily.OpenPrices, 50);
            _ema200 = Indicators.ExponentialMovingAverage(TimeFrameDaily.OpenPrices, 200);
            _rsi = Indicators.RelativeStrengthIndex(TimeFrameDaily.OpenPrices, 14);
            _ema26B = Indicators.ExponentialMovingAverage(Bars.OpenPrices, 26);
            _ema50B = Indicators.ExponentialMovingAverage(Bars.OpenPrices, 50);
            _ema200B = Indicators.ExponentialMovingAverage(Bars.OpenPrices, 200);
            _rsiB = Indicators.RelativeStrengthIndex(Bars.OpenPrices, 14);
        }

        protected override void OnBar()
        {
            //double TodayHigh = Bars.LowPrices.Maximum(Server.Time.Hour * 4);
            //double TodayLow = Bars.LowPrices.Minimum(Server.Time.Hour * 4);
            
            var TimeFrameDaily = MarketData.GetBars(TimeFrame.Daily);
            //double LastClose = TimeFrameDaily.ClosePrices.Last(1);
            if(Server.Time.Hour == 0 && Server.Time.Minute == 30)
            {
                prediction = 0;
                TradeDone = false;
            }
            
            if(Server.Time.Hour == 1 && Server.Time.Minute == 00)
            {
                RequestPrediction();
            }
            
            if(Server.Time.Hour < 5 || Server.Time.Hour > 21)
            {
                foreach (var position in Positions)
                {
                    ClosePosition(position);
                }
                return;
            }
           
            Asset baseAssetForNewOrder = Assets.GetAsset("GBP");
            Asset quoteAssetForNewOrder = Assets.GetAsset("USD");
            double BalanceUSD = baseAssetForNewOrder.Convert(quoteAssetForNewOrder, Account.Balance);
            if(Math.Floor(BalanceUSD * 200 * VolumePercent / Symbol.Ask) > 1000)
            {
                volumne = 1000;
            }
            else{volumne = Math.Floor(BalanceUSD * 200 * VolumePercent / Symbol.Ask);}
            
            if(Server.Time.Hour == 1)
            {
                DayTrade = false;
                StartBalance = Account.Balance;
                TargetMet = false;
                
            }

            if(Server.Time.Hour > 4 && 
            prediction > 0 &&
            //Server.Time.Hour < 21 && 
            //Symbol.Ask > _ema26.Result.Last(0) &&
            //Symbol.Ask > TimeFrameDaily.ClosePrices.Last(1) * (1-(MinLevel*0.01)) &&
            //Bars.ClosePrices.Last(1) > _ema26.Result.Last(0) &&
            Positions.Count == 0 
            //DayTrade == false
            )
            {
                double StopLoss = (Symbol.Ask * StopLossPercent/200);
                double TakeProfit = (Symbol.Ask * TakeProfitPercent/20/10);
                //Print("StopLoss: ", StopLoss, " TakeProfit: ", TakeProfit);
                Print(volumne);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, 2, Label, StopLoss/10, TakeProfit/10);
                DayTrade = true;
            }
            
            // else if(Server.Time.Hour > 5 && 
            // Server.Time.Hour < 21 && 
            // Symbol.Ask < EMA26.Result.Last(0) &&
            // Symbol.Ask < TFD.ClosePrices.Last(1) * (1+(MinLevel*0.01)) &&
            // Bars.ClosePrices.Last(1) < EMA26.Result.Last(0) &&
            // Positions.Count == 0 && 
            // DayTrade == false)
            // {
            //     double StopLoss = (Symbol.Ask * StopLossPercent/200);
            //     double TakeProfit = (Symbol.Ask * TakeProfitPercent/20/10);
            //     Print("StopLoss: ", StopLoss, " TakeProfit: ", TakeProfit);
            //     ExecuteMarketOrder(TradeType.Sell, SymbolName, volumne/10, Label, StopLoss/10, TakeProfit/10);
            //     DayTrade = true;
            // }
            
        }
        
        
        protected override void OnTick()
        {
           
        }
        
        
        
        
        
        
        
        
        
        
        protected void RequestPrediction()
        {
          
                if(Server.Time.DayOfWeek == DayOfWeek.Monday)
                {
                    Day1 = 0; Day2 = 4; Day3 = 3; Day4 = 2; Day5 = 1; Day6 = 0;
                }
                else if(Server.Time.DayOfWeek == DayOfWeek.Tuesday)
                {
                    Day1 = 1; Day2 = 0; Day3 = 4; Day4 = 3; Day5 = 2; Day6 = 1;
                }
                else if(Server.Time.DayOfWeek == DayOfWeek.Wednesday)
                {
                    Day1 = 2; Day2 = 1; Day3 = 0; Day4 = 4; Day5 = 3; Day6 = 2;
                }
                else if(Server.Time.DayOfWeek == DayOfWeek.Thursday)
                {
                    Day1 = 3; Day2 = 2; Day3 = 1; Day4 = 0; Day5 = 4; Day6 = 3;
                }
                else if(Server.Time.DayOfWeek == DayOfWeek.Friday)
                {
                    Day1 = 4;  Day2 = 3; Day3 = 2; Day4 = 1; Day5 = 0; Day6 = 4;
                }
            
            
            var TimeFrameDaily = MarketData.GetBars(TimeFrame.Daily);
            
            var IBS6 = (TimeFrameDaily.ClosePrices.Last(6) - TimeFrameDaily.LowPrices.Last(6)) / (TimeFrameDaily.HighPrices.Last(6) - TimeFrameDaily.LowPrices.Last(6));
            var IBS5 = (TimeFrameDaily.ClosePrices.Last(5) - TimeFrameDaily.LowPrices.Last(5)) / (TimeFrameDaily.HighPrices.Last(5) - TimeFrameDaily.LowPrices.Last(5));
            var IBS4 = (TimeFrameDaily.ClosePrices.Last(4) - TimeFrameDaily.LowPrices.Last(4)) / (TimeFrameDaily.HighPrices.Last(4) - TimeFrameDaily.LowPrices.Last(4));
            var IBS3 = (TimeFrameDaily.ClosePrices.Last(3) - TimeFrameDaily.LowPrices.Last(3)) / (TimeFrameDaily.HighPrices.Last(3) - TimeFrameDaily.LowPrices.Last(3));
            var IBS2 = (TimeFrameDaily.ClosePrices.Last(2) - TimeFrameDaily.LowPrices.Last(2)) / (TimeFrameDaily.HighPrices.Last(2) - TimeFrameDaily.LowPrices.Last(2));
            var IBS1 = (TimeFrameDaily.ClosePrices.Last(1) - TimeFrameDaily.LowPrices.Last(1)) / (TimeFrameDaily.HighPrices.Last(1) - TimeFrameDaily.LowPrices.Last(1));
            
            var Range1 = TimeFrameDaily.HighPrices.Last(1) - TimeFrameDaily.LowPrices.Last(1);
            var Range2 = TimeFrameDaily.HighPrices.Last(2) - TimeFrameDaily.LowPrices.Last(2);
            var Range3 = TimeFrameDaily.HighPrices.Last(3) - TimeFrameDaily.LowPrices.Last(3);
            var Range4 = TimeFrameDaily.HighPrices.Last(4) - TimeFrameDaily.LowPrices.Last(4);
            var Range5 = TimeFrameDaily.HighPrices.Last(5) - TimeFrameDaily.LowPrices.Last(5);
            var Range6 = TimeFrameDaily.HighPrices.Last(6) - TimeFrameDaily.LowPrices.Last(6);
            
            // Your input data as an object
            var inputDataObject = new InputData(
            Day6, TimeFrameDaily.ClosePrices.Last(6), TimeFrameDaily.TickVolumes.Last(6), Range6, IBS6, 
            _rsi.Result.Last(6), _ema26.Result.Last(6), _ema50.Result.Last(6), _ema200.Result.Last(6),
            
            Day5, TimeFrameDaily.ClosePrices.Last(5), TimeFrameDaily.TickVolumes.Last(5), Range5, IBS5, 
            _rsi.Result.Last(5), _ema26.Result.Last(5), _ema50.Result.Last(5), _ema200.Result.Last(5),
            
            Day4, TimeFrameDaily.ClosePrices.Last(4), TimeFrameDaily.TickVolumes.Last(4), Range4, IBS4, 
            _rsi.Result.Last(4), _ema26.Result.Last(4), _ema50.Result.Last(4), _ema200.Result.Last(4),
            
            Day3, TimeFrameDaily.ClosePrices.Last(3), TimeFrameDaily.TickVolumes.Last(3), Range3, IBS3, 
            _rsi.Result.Last(3), _ema26.Result.Last(3), _ema50.Result.Last(3), _ema200.Result.Last(3),
            
            Day2, TimeFrameDaily.ClosePrices.Last(2), TimeFrameDaily.TickVolumes.Last(2), Range2, IBS2, 
            _rsi.Result.Last(2), _ema26.Result.Last(2), _ema50.Result.Last(2), _ema200.Result.Last(2),
            
            Day1, TimeFrameDaily.ClosePrices.Last(1), TimeFrameDaily.TickVolumes.Last(1), Range1, IBS1, 
            _rsi.Result.Last(1), _ema26.Result.Last(1), _ema50.Result.Last(1), _ema200.Result.Last(1));
            
            // Convert the features to a JSON string
            string inputData = JsonSerializer.Serialize(inputDataObject);
            Print("inputData: ", inputData);
            
            // Replace the URL with your Flask API endpoint
                string apiUrl = "http://127.0.0.1:5000/predict";
        
                // Make an HTTP request
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                       // This is an asynchronous function
                        async Task GetPredictionAsync()
                        {
                            var content = new StringContent(inputData, Encoding.UTF8, "application/json");
                            var response = await client.PostAsync(apiUrl, content);
                            var predictionString = await response.Content.ReadAsStringAsync();
                            
                            // Extracting the integer using regular expression
                            prediction = ExtractInteger(predictionString);
                            Print(prediction);
                        }
                    
                        // Call the asynchronous function
                        GetPredictionAsync().Wait();
                    }

                }
                catch (Exception ex)
                {
                    Print("Error making HTTP request: " + ex.ToString());
                }
        }
        
        static int ExtractInteger(string jsonString)
        {
            // Using regular expression to find the first integer in the JSON string
            Match match = Regex.Match(jsonString, @"-?\d+");
    
            if (match.Success)
            {
                // Parsing the matched value to an integer
                return int.Parse(match.Value);
            }
    
            // Return a default value or handle the absence of an integer
            return 0;
        }
        
        public class InputData
        {
        public double Feature1 { get; set; }
        public double Feature2 { get; set; }
        public double Feature3 { get; set; }
        public double Feature4 { get; set; }
        public double Feature5 { get; set; }
        public double Feature6 { get; set; }
        public double Feature7 { get; set; }
        public double Feature8 { get; set; }
        public double Feature9 { get; set; }
        public double Feature10 { get; set; }
        public double Feature11 { get; set; }
        public double Feature12 { get; set; }
        public double Feature13 { get; set; }
        public double Feature14 { get; set; }
        public double Feature15 { get; set; }
        public double Feature16 { get; set; }
        public double Feature17 { get; set; }
        public double Feature18 { get; set; }
        public double Feature19 { get; set; }
        public double Feature20 { get; set; }
        public double Feature21 { get; set; }
        public double Feature22 { get; set; }
        public double Feature23 { get; set; }
        public double Feature24 { get; set; }
        public double Feature25 { get; set; }
        public double Feature26 { get; set; }
        public double Feature27 { get; set; }
        public double Feature28 { get; set; }
        public double Feature29 { get; set; }
        public double Feature30 { get; set; }
        public double Feature31 { get; set; }
        public double Feature32 { get; set; }
        public double Feature33 { get; set; }
        public double Feature34 { get; set; }
        public double Feature35 { get; set; }
        public double Feature36 { get; set; }
        public double Feature37 { get; set; }
        public double Feature38 { get; set; }
        public double Feature39 { get; set; }
        public double Feature40 { get; set; }
        public double Feature41 { get; set; }
        public double Feature42 { get; set; }
        public double Feature43 { get; set; }
        public double Feature44 { get; set; }
        public double Feature45 { get; set; }
        public double Feature46 { get; set; }
        public double Feature47 { get; set; }
        public double Feature48 { get; set; }
        public double Feature49 { get; set; }
        public double Feature50 { get; set; }
        public double Feature51 { get; set; }
        public double Feature52 { get; set; }
        public double Feature53 { get; set; }
        public double Feature54 { get; set; }        
        
        public InputData(double feature1, double feature2, double feature3, double feature4, double feature5, double feature6,
                             double feature7, double feature8, double feature9, 
                             
                             double feature10, double feature11, double feature12,
                             double feature13, double feature14, double feature15, double feature16, double feature17, double feature18,
                             
                             double feature19, double feature20, double feature21, double feature22, double feature23, double feature24,
                             double feature25, double feature26, double feature27, 
                             
                             double feature28, double feature29, double feature30, 
                             double feature31, double feature32, double feature33, double feature34, double feature35, double feature36, 
                             
                             double feature37, double feature38, double feature39, double feature40, double feature41, double feature42, 
                             double feature43, double feature44, double feature45, 
                             
                             double feature46, double feature47, double feature48,
                             double feature49, double feature50, double feature51, double feature52, double feature53, double feature54)
            {
                Feature1 = feature1;
                Feature2 = feature2;
                Feature3 = feature3;
                Feature4 = feature4;
                Feature5 = feature5;
                Feature6 = feature6;
                Feature7 = feature7;
                Feature8 = feature8;
                Feature9 = feature9;
                Feature10 = feature10;
                Feature11 = feature11;
                Feature12 = feature12;
                Feature13 = feature13;
                Feature14 = feature14;
                Feature15 = feature15;
                Feature16 = feature16;
                Feature17 = feature17;
                Feature18 = feature18;
                Feature19 = feature19;
                Feature20 = feature20;
                Feature21 = feature21;
                Feature22 = feature22;
                Feature23 = feature23;
                Feature24 = feature24;
                Feature25 = feature25;
                Feature26 = feature26;
                Feature27 = feature27;
                Feature28 = feature28;
                Feature29 = feature29;
                Feature30 = feature30;
                Feature31 = feature31;
                Feature32 = feature32;
                Feature33 = feature33;
                Feature34 = feature34;
                Feature35 = feature35;
                Feature36 = feature36;
                Feature37 = feature37;
                Feature38 = feature38;
                Feature39 = feature39;
                Feature40 = feature40;
                Feature41 = feature41;
                Feature42 = feature42;
                Feature43 = feature43;
                Feature44 = feature44;
                Feature45 = feature45;
                Feature46 = feature46;
                Feature47 = feature47;
                Feature48 = feature48;
                Feature49 = feature49;
                Feature50 = feature50;
                Feature51 = feature51;
                Feature52 = feature52;
                Feature53 = feature53;
                Feature54 = feature54;
            }
        }
    }
}