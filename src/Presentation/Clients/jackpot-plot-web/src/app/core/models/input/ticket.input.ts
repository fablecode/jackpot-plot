export interface TicketInput {
  name: string;
  plays?: {
    lineIndex: number;
    numbers: number[];
  }[];
}
