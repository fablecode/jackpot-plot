export interface TicketInput {
  name: string;
  lotteryId: number;
  plays?: {
    lineIndex: number;
    numbers: number[];
  }[];
}
