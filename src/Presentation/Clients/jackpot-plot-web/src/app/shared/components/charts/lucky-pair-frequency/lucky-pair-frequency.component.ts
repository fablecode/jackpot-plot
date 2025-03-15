import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {ChartComponent} from 'ngx-apexcharts';
import {ChartOptions} from '../../../../core/models/chart.options.type';
import {HotColdNumbersService} from '../hot-cold-numbers/hot-cold-numbers.service';
import {LuckyPair} from '../../../../core/models/lucky-pair';
import {LuckyPairFrequencyService} from './lucky-pair-frequency.service';
import {NgIf} from '@angular/common';

@Component({
  selector: 'app-lucky-pair-frequency',
  imports: [
    NgIf,
    ChartComponent
  ],
  templateUrl: './lucky-pair-frequency.component.html',
  styleUrl: './lucky-pair-frequency.component.scss'
})
export class LuckyPairFrequencyComponent implements OnInit, AfterViewInit {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;

  constructor(private luckyPairFrequencyService: LuckyPairFrequencyService) {}

  ngOnInit(): void {
    // ✅ Initialize chart with empty data (avoids undefined errors)
    this.initChart();
  }

  ngAfterViewInit(): void {
    // ✅ Subscribe to a single observable
    this.luckyPairFrequencyService.getLuckNumberFrequencies().subscribe((data: LuckyPair[]) => {
      this.updateChart(data);
    });
  }

  initChart(): void {
    this.chartOptions = {
      series: [{
        name: "",
        data: []
      }],
      chart: {
        type: "heatmap",
        height: 350,
        zoom: { enabled: false }, // ✅ Prevents zoom-related touch conflicts
        animations: { enabled: true }, // ✅ Disables animations to reduce event listener errors
        toolbar: { show: false }, // ✅ Removes the toolbar to prevent zoom issues
      },
      dataLabels: {
        enabled: true,
        style: { fontSize: '12px', colors: ['#fff'] } // ✅ Improved text readability
      },
      colors: ["#008FFB"], // ✅ Ensures darker colors for high frequency
      title: {
        text: "🔗 Lucky Pair Frequency",
        align: "center"
      },
      xaxis: {
        title: { text: "Lucky Pairs" },
        labels: { rotate: -45 } // ✅ Improve readability
      },
      yaxis: {
        title: { text: "Frequency" }
      },
      tooltip: {
        enabled: true,
        y: {
          formatter: (val) => {
            if (val >= 1_000_000) return (val / 1_000_000).toFixed(1) + "M"; // ✅ Millions
            if (val >= 1_000) return (val / 1_000).toFixed(1) + "K"; // ✅ Thousands
            return val.toString();
          }
        }
      },
      plotOptions: {
        heatmap: {
          shadeIntensity: 0.5, // ✅ Controls how dark high values appear
          colorScale: {
            ranges: [
              { from: 0, to: 50, color: "#B3E5FC", name: "Low" },
              { from: 51, to: 100, color: "#0288D1", name: "Medium" },
              { from: 101, to: 200, color: "#01579B", name: "High" }
            ]
          }
        }
      }
    };
  }

  updateChart(luckNumberFrequencies: LuckyPair[]): void {
    if (this.chart) {
      const heatmapData = luckNumberFrequencies.map(pair => ({ x: `${pair.number1} & ${pair.number2}`, y: pair.frequency }));

      const newColorScale = this.generateColorScale(luckNumberFrequencies);

      // ✅ Function to get color category name based on frequency
      const getColorCategory = (val: number) => {
        const category = newColorScale.find(range => val >= range.from && val <= range.to);
        return category ? category.name : "Unknown";
      };

      const formatNumber = (num: number) => {
        if (num >= 1_000_000) return (num / 1_000_000).toFixed(1) + "M"; // ✅ Millions
        if (num >= 1_000) return (num / 1_000).toFixed(1) + "K"; // ✅ Thousands
        return num.toString(); // ✅ Regular number
      };


      this.chart.updateSeries([
        { name: "", data: heatmapData }
      ]);

      this.chart.updateOptions({
        plotOptions: {
          heatmap: {
            shadeIntensity: 0.5,
            colorScale: {
              ranges: newColorScale
            }
          }
        },
        tooltip: {
          enabled: true,
          custom: ({ series, seriesIndex, dataPointIndex, w }) => {
            const value = series[seriesIndex][dataPointIndex];
            const pairLabel = w.globals.labels[dataPointIndex];
            const category = getColorCategory(value);
            const formattedValue = formatNumber(value);

            return `
          <div style="background: #fff; padding: 10px; border-radius: 5px; box-shadow: 0px 0px 5px rgba(0, 0, 0, 0.2);">
            <strong style="font-size: 14px;">🔗 ${pairLabel}</strong>
            <br/>
            <span style="font-size: 12px;">Frequency: <strong>${formattedValue}</strong></span>
            <br/>
            <span style="font-size: 12px;">Category: <span style="color: ${newColorScale.find(r => r.name === category)?.color}; font-weight: bold;">${category}</span></span>
          </div>
        `;
          }
        }
      });
    }
  }

  generateColorScale(data: LuckyPair[]) {
    const minFrequency = Math.min(...data.map(pair => pair.frequency));
    const maxFrequency = Math.max(...data.map(pair => pair.frequency));
    const range = maxFrequency - minFrequency;

    return [
      { from: minFrequency, to: minFrequency + range * 0.25, color: "#440154", name: "Rare" },  // Dark Purple
      { from: minFrequency + range * 0.26, to: minFrequency + range * 0.50, color: "#3B528B", name: "Uncommon" },  // Blue
      { from: minFrequency + range * 0.51, to: minFrequency + range * 0.75, color: "#21918C", name: "Frequent" },  // Greenish
      { from: minFrequency + range * 0.76, to: maxFrequency, color: "#FDE725", name: "Hot Pairs 🔥" }  // Bright Yellow
    ];
  }
}
