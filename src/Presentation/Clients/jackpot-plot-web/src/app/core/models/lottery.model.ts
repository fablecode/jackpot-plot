export interface Lottery {
  id: number;
  name: string;
  drawDate: string; // Use `Date` if the API returns ISO dates
  jackpot: number;
  numbersDrawn: number[];
}
