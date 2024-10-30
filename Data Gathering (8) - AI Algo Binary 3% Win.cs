using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class DataGathering8AIAlgoBinary3Win : Robot
    {
        private ExponentialMovingAverage _ema26;
        private ExponentialMovingAverage _ema50;
        private ExponentialMovingAverage _ema200;
        private RelativeStrengthIndex _rsi;
        
        [Parameter("Gain %", DefaultValue = 3)]
        public double Gain { get; set; }
        
        [Parameter("Loss %", DefaultValue = 10)]
        public double Loss { get; set; }
        
        [Parameter("High % x 10000", DefaultValue = 100)]
        public int RisePercent { get; set; }
        
        [Parameter("Low % x 10000", DefaultValue = 100)]
        public int DropPercent { get; set; }

        public int wins;
        public int loses;
        public int nopred;        
        public int Outcome;
        public double Highest;
        public double Lowest;

        protected override void OnStart()
        {
            var TimeFrameDaily = MarketData.GetBars(TimeFrame.Daily);
            _ema26 = Indicators.ExponentialMovingAverage(TimeFrameDaily.ClosePrices, 26);
            _ema50 = Indicators.ExponentialMovingAverage(TimeFrameDaily.ClosePrices, 50);
            _ema200 = Indicators.ExponentialMovingAverage(TimeFrameDaily.ClosePrices, 200);
            _rsi = Indicators.RelativeStrengthIndex(TimeFrameDaily.ClosePrices, 14);
        }

        protected override void OnBar()
        {
            if(Server.Time.Hour == 21 && Server.Time.Minute == 0)
            {
                Highest = Bars.HighPrices.Maximum(20*12);
                Lowest = Bars.LowPrices.Minimum(20*12);
            }
            
            var TimeFrameDaily = MarketData.GetBars(TimeFrame.Daily);
            if(Server.Time.Hour == 21 && Server.Time.Minute == 00)
            {
            int currentDay = (int)Server.Time.DayOfWeek;
        
            var IBS1 = (TimeFrameDaily.ClosePrices.Last(1) - TimeFrameDaily.LowPrices.Last(1)) / (TimeFrameDaily.HighPrices.Last(1) - TimeFrameDaily.LowPrices.Last(1));
            
            var Range1 = TimeFrameDaily.HighPrices.Last(1) - TimeFrameDaily.LowPrices.Last(1);
            
            //Print("Highest: ",Highest, " 5AM Close: ", Bars.ClosePrices.Last(16*12), " Calc: ", Bars.ClosePrices.Last(16*12) * (1+(Gain/2000)));
            //Print("Lowest: ",Lowest, " 5AM Close: ", Bars.ClosePrices.Last(16*12), " Calc: ", Bars.ClosePrices.Last(16*12) * (1-(Loss/2000)));
                
            //Result
            if(Highest > Bars.ClosePrices.Last(16*12) * (1+(Gain/2000)) && Lowest > Bars.ClosePrices.Last(16*12) * (1-(Loss/2000)))
            {
                
                Outcome = 1;
                wins += 1;
            }
            
            else
            {
                Outcome = -1;
                nopred +=1;
            }
            
            Print(currentDay, "/",TimeFrameDaily.ClosePrices.Last(1), "/",TimeFrameDaily.TickVolumes.Last(1), "/",Range1, "/",IBS1, "/",_rsi.Result.Last(1), "/",_ema26.Result.Last(1), "/",_ema50.Result.Last(1), "/",_ema200.Result.Last(1), "/", Outcome); 
            
            }
        }

        protected override void OnTick()
        {
            // Handle price updates here
        }

        protected override void OnStop()
        {
            Print("Pred Up: ", wins);
            //Print("Pred down: ", loses);
            Print("No Pred: ", nopred);
        }
    }
}