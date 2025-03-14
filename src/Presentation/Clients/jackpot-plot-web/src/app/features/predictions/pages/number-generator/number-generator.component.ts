import {Component, OnInit} from '@angular/core';
import {LotteryService} from '../../../../core/services/lottery.service';
import {Lottery} from '../../../../core/models/lottery.model';
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {CommonModule, NgForOf} from '@angular/common';
import {Strategy} from '../../../../core/models/strategy.model';
import {PredictionService} from '../../../../core/services/prediction.service';

import {LotterySearchResult} from '../../../../core/models/lotterySearchResult';
import {
  HotColdNumbersComponent
} from '../../../../shared/components/charts/hot-cold-numbers/hot-cold-numbers.component';
import {HotColdNumbersService} from '../../../../shared/components/charts/hot-cold-numbers/hot-cold-numbers.service';
import {
  TrendingNumbersComponent
} from '../../../../shared/components/charts/trending-numbers/trending-numbers.component';
import {TrendingNumbersService} from '../../../../shared/components/charts/trending-numbers/trending-numbers.service';
import {
  PredictionSuccessRateComponent
} from '../../../../shared/components/charts/prediction-success-rate/prediction-success-rate.component';
import {
  PredictionSuccessRateService
} from '../../../../shared/components/charts/prediction-success-rate/prediction-success-rate.service';
import {forkJoin, tap} from 'rxjs';
import {
  NumberSpreadAnalysisComponent
} from '../../../../shared/components/charts/number-spread-analysis/number-spread-analysis.component';
import {
  NumberSpreadAnalysisService
} from '../../../../shared/components/charts/number-spread-analysis/number-spread-analysis.service';
import {
  LuckyPairFrequencyService
} from '../../../../shared/components/charts/lucky-pair-frequency/lucky-pair-frequency.service';
import {
  LuckyPairFrequencyComponent
} from '../../../../shared/components/charts/lucky-pair-frequency/lucky-pair-frequency.component';
import {
  WinningNumberFrequencyService
} from '../../../../shared/components/charts/winning-number-frequency/winning-number-frequency.service';
import {
  WinningNumberFrequencyComponent
} from '../../../../shared/components/charts/winning-number-frequency/winning-number-frequency.component';

@Component({
  selector: 'app-number-generator',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HotColdNumbersComponent,
    TrendingNumbersComponent,
    PredictionSuccessRateComponent,
    NumberSpreadAnalysisComponent,
    LuckyPairFrequencyComponent,
    WinningNumberFrequencyComponent
  ],
  templateUrl: './number-generator.component.html',
  styleUrl: './number-generator.component.scss'
})
export class NumberGeneratorComponent implements OnInit {
  generateNumbersForm: FormGroup;
  lotteryOptions: Lottery[] = []; // Now using the `Lottery` model
  numberOfPlaysOptions: number[] = []; // Static list for number of plays
  strategyOptions: Strategy[] = [];
  isShaking: boolean = false; // Controls the shake animation
  isLoadingPredictions = false;
  searchResults: LotterySearchResult | null = null;

  isLoadingLotteries: boolean = true;
  isLoadingStrategies: boolean = true;

  showCharts = false;

  constructor(
    private lotteryService: LotteryService, private predictionService: PredictionService,
    private hotColdNumbersService: HotColdNumbersService,
    private trendingNumbersService: TrendingNumbersService,
    private predictionSuccessRateService: PredictionSuccessRateService,
    private numberSpreadAnalysisService: NumberSpreadAnalysisService,
    private luckNumberFrequencyService: LuckyPairFrequencyService,
    private winningNumberFrequencyService: WinningNumberFrequencyService,
  ) {
    this.generateNumbersForm = new FormGroup({
      selectedLottery: new FormControl('', Validators.required),
      selectedNumberOfPlays: new FormControl(5, Validators.required),
      selectedStrategy: new FormControl('random', Validators.required)
    })
  }

  onSearch() {
    if (this.generateNumbersForm.valid) {
      this.isLoadingPredictions = true;
      this.searchResults = null;

      this.predictionService.searchLottery(this.generateNumbersForm.value.selectedLottery, this.generateNumbersForm.value.selectedNumberOfPlays, this.generateNumbersForm.value.selectedStrategy)
        .subscribe({
          next: (data) => {
            this.searchResults = data;
            this.isLoadingPredictions = false;
            this.showCharts = true;

            // Load additional data and wait for all to complete
            this.loadCharts().subscribe({
              next: () => {
                this.isLoadingPredictions = false; // Hide loader after everything is done
              },
              error: (error) => {
                console.error('Error fetching additional data:', error);
                this.isLoadingPredictions = false; // Hide loader even if there's an error
              }
            });
          },
          error: (error) => {
            console.error('Error fetching lottery predictions:', error);
            this.isLoadingPredictions = false;
          },
          complete: () => {
            console.log('Search completed.');
          }
        });
    } else {
      console.log('Form is invalid');
    }
  }

  loadCharts() {

    const hotColdNumbers$ = this.predictionService.getHotColdNumbers(this.generateNumbersForm.value.selectedLottery);
    const trendingNumbers$ = this.predictionService.getTrendingNumbers();
    const predictionSuccessRate$ = this.predictionService.getPredictionSuccessRate();
    const numberSpread$ = this.predictionService.getNumberSpread();
    const luckyNumberFrequency$ = this.predictionService.getLuckyPairFrequency();
    const winningNumberFrequencies$ = this.predictionService.getWinningNumberFrequency();

    return forkJoin([hotColdNumbers$, trendingNumbers$, predictionSuccessRate$, numberSpread$, luckyNumberFrequency$, winningNumberFrequencies$]).pipe(
      tap(([hotColdData, trendingData, successRateData, numberSpreadData, luckNumberFrequencyData, winningNumberFrequenciesData]) => {
        // Update respective services
        this.hotColdNumbersService.updateNumbers(hotColdData.hotNumbers, hotColdData.coldNumbers);
        this.trendingNumbersService.updateTrendingNumbers(trendingData);
        this.predictionSuccessRateService.updatePredictionSuccessRate(successRateData);
        this.numberSpreadAnalysisService.updateNumberSpreadAnalysis(numberSpreadData);
        this.luckNumberFrequencyService.updateLuckNumberFrequencies(luckNumberFrequencyData);
        this.winningNumberFrequencyService.updateWinningNumberFrequencies(winningNumberFrequenciesData);
      })
    );
  }

  ngOnInit(): void {

    this.lotteryService.getAllLotteries().subscribe({
      next: (data: Lottery[]) => {
        this.lotteryOptions = data;
        this.isLoadingLotteries = false;
      },
      error: (err) => {
        console.error('Error fetching lotteries:', err);
        this.isLoadingLotteries = false;
      }
    });

    this.predictionService.getAllStrategies().subscribe({
      next: (data: Strategy[]) => {
        this.strategyOptions = data;
        this.isLoadingStrategies = false;
      },
      error: (err) => {
        console.error('Error fetching strategies:', err);
        this.isLoadingStrategies = false;
      }
    });

    // Populate static number of plays list (1–10, 15, 20, 25)
    this.numberOfPlaysOptions = [...Array(10).keys()].map(i => i + 1).concat([15, 20, 25]);
    }

  isFieldInvalid(field: string): boolean {
    const control = this.generateNumbersForm.get(field);
    return control?.invalid && control?.touched;
  }
}
