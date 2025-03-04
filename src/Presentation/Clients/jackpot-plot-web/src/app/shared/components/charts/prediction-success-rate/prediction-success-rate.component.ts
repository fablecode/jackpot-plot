import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {ChartComponent, NgxApexchartsModule, ApexPlotOptions} from 'ngx-apexcharts';
import {ChartOptions} from '../../../../core/models/chart.options.type';
import {PredictionSuccessRateService} from './prediction-success-rate.service';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-prediction-success-rate',
  imports: [CommonModule, NgxApexchartsModule],
  templateUrl: './prediction-success-rate.component.html',
  styleUrl: './prediction-success-rate.component.scss'
})
export class PredictionSuccessRateComponent implements OnInit, AfterViewInit {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;

  constructor(private predictionSuccessRateService: PredictionSuccessRateService) {}

  initChart(): void {
    this.chartOptions = {
      series: [{
        name: "Frequency",
        data: [] //matches.map((count, index) => ({ x: count, y: index + 1 })) // ✅ Match count as x-axis, frequency as y-axis
      }],
      chart: {
        type: "bar",
        height: 350
      },
      plotOptions: { // ✅ Explicitly define type for plotOptions
        bar: {
          columnWidth: "80%", // ✅ Adjust column width for histogram look
          borderRadius: 3
        }
      } as ApexPlotOptions, // ✅ Cast `plotOptions` to ApexPlotOptions
      xaxis: {
        type: "numeric", // ✅ Uses numeric bins instead of dates
        categories: [],//[...new Set(matches)], // ✅ Unique match counts as bins
        title: { text: "Number of Matches" },
        //tickAmount: matches.length > 6 ? 6 : matches.length // ✅ Prevents overcrowding
      },
      yaxis: {
        title: { text: "Frequency (Number of Predictions)" }
      },
      title: {
        text: "🎯 Prediction Success Rate (Histogram)",
        align: "center"
      },
      dataLabels: {
        enabled: false
      },
      legend: {
        show: false
      },
      stroke: {
        show: true,
        width: 2
      },
      tooltip: {
        enabled: true,
        x: { formatter: (val) => `Matched ${val} Numbers` }, // ✅ Custom tooltip format
        y: { formatter: (val) => `${val} Predictions` } // ✅ Show number of predictions for each bin
      }
    };
  }

  updateChart(predictionSuccessRates: { matchCount: number; frequency: number }[]): void {

    if (!this.chart || !predictionSuccessRates.length) {
      console.warn("Chart is not initialized or no data available.");
      return;
    }

    const uniqueMatchCounts = [...new Set(predictionSuccessRates.map(d => d.matchCount))].sort((a, b) => a - b); // ✅ Ensure unique & sorted match bins
    const frequencies = uniqueMatchCounts.map(match => predictionSuccessRates.find(d => d.matchCount === match)?.frequency || 0); // ✅ Map match counts to frequencies

    // ✅ Update series data
    this.chart.updateSeries([
      { name: "Match Count", data: uniqueMatchCounts.map((match, index) => ({ x: match, y: frequencies[index] })) }
    ]);

    // ✅ Convert into a histogram-style chart
    this.chart.updateOptions({
      chart: {
        type: "bar",
        height: 350
      },
      plotOptions: {
        bar: {
          columnWidth: "80%", // ✅ Adjust column width for histogram look
          borderRadius: 3
        }
      },
      xaxis: {
        type: "numeric", // ✅ Numeric bins (match counts)
        categories: [...new Set(uniqueMatchCounts)], // ✅ Unique match counts as bins
        title: { text: "Number of Matches" },
        tickAmount: uniqueMatchCounts.length > 6 ? 6 : uniqueMatchCounts.length, // ✅ Prevents overcrowding
      },
      yaxis: {
        title: { text: "Frequency (Number of Predictions)" }
      },
      title: {
        text: "🎯 Prediction Success Rate (Histogram)",
        align: "center"
      }
    });

  }

  ngAfterViewInit(): void {
    // ✅ Subscribe to a single observable for prediction success rates
    this.predictionSuccessRateService.getPredictionSuccessRate().subscribe((predictionSuccessRates) => {
      this.updateChart(predictionSuccessRates);
    });
  }

  ngOnInit(): void {
    // ✅ Initialize chart with empty data (avoids undefined errors)
    this.initChart();
  }
}
