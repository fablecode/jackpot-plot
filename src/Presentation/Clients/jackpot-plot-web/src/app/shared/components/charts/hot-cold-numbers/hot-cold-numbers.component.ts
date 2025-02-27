import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {ChartComponent, NgxApexchartsModule} from 'ngx-apexcharts';
import {CommonModule} from '@angular/common';
import {HotColdNumbersService} from './hot-cold-numbers.service';
import {ChartOptions} from '../../../../core/models/chart.options.type';

@Component({
  selector: 'app-hot-cold-numbers',
  imports: [CommonModule, NgxApexchartsModule],
  templateUrl: './hot-cold-numbers.component.html',
  styleUrl: './hot-cold-numbers.component.scss'
})
export class HotColdNumbersComponent implements OnInit, AfterViewInit {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;

  constructor(private hotColdNumbersService: HotColdNumbersService) {}

  ngOnInit(): void {
    // ✅ Initialize chart with empty data (avoids undefined errors)
    this.initChart();
  }

  ngAfterViewInit(): void {
    // ✅ Subscribe to a single observable for both hot & cold numbers
    this.hotColdNumbersService.getNumbers().subscribe(({ hotNumbers, coldNumbers }) => {
      this.updateChart(hotNumbers, coldNumbers);
    });
  }

  initChart(): void {
    this.chartOptions = {
      series: [
        { name: "Hot Numbers 🔥", data: [] },
        { name: "Cold Numbers ❄️", data: [] }
      ],
      chart: { type: "bar", height: 350, stacked: false, animations: { enabled: true } },
      xaxis: { title: { text: "Number" }},
      yaxis: { title: { text: "Frequency" } },
      title: { text: "🔥 Hot & ❄️ Cold Numbers", align: "center" },
      dataLabels: { enabled: false },
      legend: { show: false },
      stroke: { show: true, width: 2 },
      tooltip: { enabled: true }
    };
  }

  updateChart(hotNumbers: Record<number, number>, coldNumbers: Record<number, number>): void {
    if (this.chart) {
      this.chart.updateSeries([
        { name: "Hot Numbers 🔥", data: Object.entries(hotNumbers).map(([num, count]) => ({ x: Number(num), y: count })) },
        { name: "Cold Numbers ❄️", data: Object.entries(coldNumbers).map(([num, count]) => ({ x: Number(num), y: count })) }
      ]);
    }
  }

}
