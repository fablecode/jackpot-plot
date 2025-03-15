import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {ChartComponent, NgxApexchartsModule} from 'ngx-apexcharts';
import {ChartOptions} from '../../../../core/models/chart.options.type';
import {NumberSpreadAnalysisService} from './number-spread-analysis.service';
import {NumberSpread} from '../../../../core/models/numberSpread';
import {CommonModule, NgIf} from '@angular/common';

@Component({
  selector: 'app-number-spread-analysis',
  imports: [CommonModule, NgxApexchartsModule],
  templateUrl: './number-spread-analysis.component.html',
  styleUrl: './number-spread-analysis.component.scss'
})
export class NumberSpreadAnalysisComponent implements OnInit, AfterViewInit {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;

  constructor(private numberSpreadAnalysisService: NumberSpreadAnalysisService) {}

  initChart(): void {
    this.chartOptions = {
      series: [
        { name: "Low (1-20)", data: [] },
        { name: "Mid (21-40)", data: [] },
        { name: "High (41-50)", data: [] }
      ],
      chart: { type: "bar", height: 350 }, // ✅ Stacked bar chart
      xaxis: { categories: ["Number Distribution"], title: { text: "Number Ranges" } },
      yaxis: { title: { text: "Frequency1" } },
      title: { text: "📊 Number Spread Analysis", align: "center" },
      dataLabels: { enabled: false },
      legend: { position: "top" },
      tooltip: {
        enabled: true,
        y: { formatter: (val) => `${val} numbers` }
      }
    };
  }

  updateChart(numberSpread: NumberSpread): void {
    if (this.chart) {
      this.chart.updateSeries([
        { name: "Low (1-20)", data: [numberSpread.low] },
        { name: "Mid (21-40)", data: [numberSpread.mid] },
        { name: "High (41-50)", data: [numberSpread.high] }
      ]);
    }
  }

  ngOnInit(): void {
    // ✅ Initialize chart with empty data (avoids undefined errors)
    this.initChart();
  }

  ngAfterViewInit(): void {
    // ✅ Subscribe to a single observable
    this.numberSpreadAnalysisService.getNumberSpreadAnalysis().subscribe((numberSpread) => {
      this.updateChart(numberSpread);
    });
  }
}
