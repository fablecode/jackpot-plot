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

@Component({
  selector: 'app-number-generator',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HotColdNumbersComponent,
    TrendingNumbersComponent
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

  lotteryHotNumbers = {};
  lotteryColdNumbers = {};

  isLoadingLotteries: boolean = true;
  isLoadingStrategies: boolean = true;

  showCharts = false;

  constructor(private lotteryService: LotteryService, private predictionService: PredictionService, private hotColdNumbersService: HotColdNumbersService, private trendingNumbersService: TrendingNumbersService) {
    this.generateNumbersForm = new FormGroup({
      selectedLottery: new FormControl('', Validators.required),
      selectedNumberOfPlays: new FormControl(5, Validators.required),
      selectedStrategy: new FormControl('random', Validators.required)
    })
  }

  onSearch() {
    if (this.generateNumbersForm.valid) {
      this.isLoadingPredictions = true;

      this.predictionService.searchLottery(this.generateNumbersForm.value.selectedLottery, this.generateNumbersForm.value.selectedNumberOfPlays, this.generateNumbersForm.value.selectedStrategy)
        .subscribe({
          next: (data) => {
            this.searchResults = data;
            this.isLoadingPredictions = false;
            this.showCharts = true;
            this.loadCharts();
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
    this.predictionService.getHotColdNumbers(this.generateNumbersForm.value.selectedLottery)
      .subscribe({
        next: (data) => {
          this.hotColdNumbersService.updateNumbers(data.hotNumbers, data.coldNumbers);
        },
        error: (error) => {
          console.error('Error fetching Hot & Cold numbers:', error);
        },
        complete: () => {
          console.log('Loading Hot & Cold numbers completed.');
        }
      });

    this.predictionService.getTrendingNumbers()
      .subscribe({
        next: (data) => {
          this.trendingNumbersService.updateTrendingNumbers(data);
        },
        error: (error) => {
          console.error('Error fetching Trending numbers:', error);
        },
        complete: () => {
          console.log('Loading Trending numbers completed.');
        }
      });
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

  triggerShake() {
    this.isShaking = true;
    setTimeout(() => {
      this.isShaking = false;
    }, 300); // Shake duration (matches CSS animation)
  }
}
