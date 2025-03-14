import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ChartComponent, NgxApexchartsModule} from 'ngx-apexcharts';
import {ChartOptions} from '../../../../core/models/chart.options.type';
import {WinningNumberFrequencyService} from './winning-number-frequency.service';
import {WinningNumberFrequency} from '../../../../core/models/winning-number-frequency.model';

@Component({
  selector: 'app-winning-number-frequency',
  imports: [CommonModule, NgxApexchartsModule],
  templateUrl: './winning-number-frequency.component.html',
  styleUrl: './winning-number-frequency.component.scss'
})
export class WinningNumberFrequencyComponent implements OnInit, AfterViewInit {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;

  constructor(private winningNumberFrequencyService: WinningNumberFrequencyService) {}

  ngOnInit(): void {
    // ✅ Initialize chart with empty data (avoids undefined errors)
    this.initChart();
  }

  ngAfterViewInit(): void {
    // ✅ Subscribe to a single observable
    this.winningNumberFrequencyService.getWinningNumberFrequencies().subscribe((winningNumberFrequencies) => {
      this.updateChart(winningNumberFrequencies);
    });
  }

  initChart(): void {
    this.chartOptions = {
      series: [],
      chart: { type: "area", height: 350 },
      xaxis: {
        type: "datetime",
        title: { text: "Date" }
      },
      yaxis: { title: { text: "Frequency Over Time" } },
      title: { text: "📉 Winning Number Frequency Over Time", align: "center" },
      stroke: { curve: "smooth", width: 2 },
      fill: { type: "gradient" },
      markers: { size: 3 },
      tooltip: {
        enabled: true,
        x: { format: "dd MMM yyyy" }
      }
    };
  }

  updateChart(winningNumberFrequencies: WinningNumberFrequency[]): void {
    if (this.chart) {

      const seriesData = winningNumberFrequencies.map(num => ({
        name: `Number ${num.number}`,
        data: Object.entries(num.frequencyOverTime).map(([date, freq]) => ({
          x: new Date(date).getTime(),
          y: freq
        }))
      }));

      this.chart.updateSeries(seriesData);

      // ✅ Dynamically Update Y-Axis Scale Based on New Data
      const maxFrequency = Math.max(...winningNumberFrequencies.flatMap(num => Object.values(num.frequencyOverTime)));

      this.chart.updateOptions({
        yaxis: {
          title: { text: "Total Appearances in Draws" },
          min: 0,
          max: maxFrequency + 1 // ✅ Dynamically adjust max based on data
        }
      });
    }
  }
}
