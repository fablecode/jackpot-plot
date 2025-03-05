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
      series: [{ name: "Frequency", data: [] }],
      chart: { type: "heatmap", height: 350, animations: { enabled: true } },
      xaxis: { title: { text: "Lucky Pairs" } },
      yaxis: { title: { text: "Frequency" } },
      title: { text: "🔗 Lucky Pair Frequency", align: "center" },
      dataLabels: { enabled: false },
      colors: ["#008FFB"],
      tooltip: {
        enabled: true,
        y: { formatter: (val) => `${val} times` }
      }
    };
  }

  updateChart(luckNumberFrequencies: LuckyPair[]): void {
    if (this.chart) {
      const heatmapData = luckNumberFrequencies.map(pair => ({ x: `${pair.number1} & ${pair.number2}`, y: pair.frequency }));

      this.chart.updateSeries([
        { name: "Frequency", data: heatmapData }
      ]);
    }
  }
}
