import {Prediction} from "./prediction";

export interface Play {
  playNumber: number;
  predictions: Prediction[];
  isBookmarked: boolean;
  bookmarkAnimation?: boolean; // used for animation only
  bookmarkLoading?: boolean;
}
