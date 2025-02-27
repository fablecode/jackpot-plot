import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ChartComponent, NgxApexchartsModule} from 'ngx-apexcharts';
import {ChartOptions} from '../../../../core/models/chart.options.type';
import {HotColdNumbersService} from '../hot-cold-numbers/hot-cold-numbers.service';
import {TrendingNumbersService} from './trending-numbers.service';

@Component({
  selector: 'app-trending-numbers',
  imports: [CommonModule, NgxApexchartsModule],
  templateUrl: './trending-numbers.component.html',
  styleUrl: './trending-numbers.component.scss'
})
export class TrendingNumbersComponent implements OnInit, AfterViewInit {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;

  constructor(private trendingNumbersService: TrendingNumbersService) {}

  initChart(): void {
    this.chartOptions = {
      series: [{ name: "Trending Numbers", data: [] }],
      chart: { type: "bar", height: 350, stacked: false, animations: { enabled: true } },
      xaxis: { title: { text: "Numbers" } },
      yaxis: { title: { text: "Frequency" } },
      title: { text: "📈 Trending Lucky Numbers", align: "center" },
      dataLabels: { enabled: false },
      legend: { show: true },
      stroke: { show: true, width: 2 },
      tooltip: { enabled: true }
    };
  }

  updateChart(trendingNumbers: Record<number, number>): void {
    if (this.chart) {
      this.chart.updateSeries([
        { name: "Trending Numbers", data: Object.entries(trendingNumbers).map(([num, count]) => ({ x: Number(num), y: count })) }
      ]);
    }
  }

  ngOnInit(): void {
    // ✅ Initialize chart with empty data (avoids undefined errors)
    this.initChart();
  }

  ngAfterViewInit(): void {
    // ✅ Subscribe to a single observable for both hot & cold numbers
    this.trendingNumbersService.getTrendingNumbers().subscribe((trendingNumbers) => {
      this.updateChart(trendingNumbers);
    });
  }
}
