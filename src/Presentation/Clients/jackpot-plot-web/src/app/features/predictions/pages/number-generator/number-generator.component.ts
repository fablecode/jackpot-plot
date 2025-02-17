import {Component, OnInit} from '@angular/core';
import {LotteryService} from '../../../../core/services/lottery.service';
import {Lottery} from '../../../../core/models/lottery.model';
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {CommonModule, NgForOf} from '@angular/common';
import {Strategy} from '../../../../core/models/strategy.model';
import {PredictionService} from '../../../../core/services/prediction.service';

@Component({
  selector: 'app-number-generator',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
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

  constructor(private lotteryService: LotteryService, private predictionService: PredictionService) {
    this.generateNumbersForm = new FormGroup({
      selectedLottery: new FormControl('', Validators.required),
      selectedNumberOfPlays: new FormControl(5, Validators.required),
      selectedStrategy: new FormControl('random', Validators.required)
    })
  }

  onSubmit() {
    if (this.generateNumbersForm.valid) {
      const selectedLottery = this.lotteryOptions.find(
        (lottery) => lottery.id === +this.generateNumbersForm.value.selectedLottery
      );
      console.log('Selected Lottery:', selectedLottery);
      console.log('Selected number of plays:', this.generateNumbersForm.value.selectedNumberOfPlays);
      console.log('Selected strategy:', this.generateNumbersForm.value.selectedStrategy);
    } else {
      console.log('Form is invalid');
    }
  }

  ngOnInit(): void {
    this.lotteryService.getAllLotteries().subscribe({
      next: (data: Lottery[]) => {
        this.lotteryOptions = data;
      },
      error: (err) => console.error('Error fetching lotteries:', err)
    });

    this.predictionService.getAllStrategies().subscribe({
      next: (data: Strategy[]) => {
        this.strategyOptions = data;
      },
      error: (err) => console.error('Error fetching strategies:', err)
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
