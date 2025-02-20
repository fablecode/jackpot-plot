import {Play} from "./play";

export interface LotterySearchResult {
  lotteryId: number;
  numberOfPlays: number;
  strategy: string;
  predictions: Play[];
}
