import {Component, OnInit} from '@angular/core';
import {LotteryService} from '../../../../core/services/lottery.service';
import {Lottery} from '../../../../core/models/lottery.model';
import {FormsModule} from '@angular/forms';
import {CommonModule, NgForOf} from '@angular/common';

@Component({
  selector: 'app-number-generator',
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './number-generator.component.html',
  styleUrl: './number-generator.component.scss'
})
export class NumberGeneratorComponent implements OnInit {
  lotteries: Lottery[] = []; // Now using the `Lottery` model

  selectedLottery: number | null = null;
  loading = true; // Flag for loading state

  constructor(private lotteryService: LotteryService) {}

  onLotteryChange(event: Event) {
    const selectedValue = (event.target as HTMLSelectElement).value;
    this.selectedLottery = Number(selectedValue);
    console.log('Selected Lottery ID:', this.selectedLottery);
  }

  ngOnInit(): void {
      this.lotteryService.getAllLotteries().subscribe({
        next: (data: Lottery[]) => {
          this.lotteries = data;
          this.loading = false;
        },
        error: (err) => console.error('Error fetching lotteries:', err)
      });
    }
}
