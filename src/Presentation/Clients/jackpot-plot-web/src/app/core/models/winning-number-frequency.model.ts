export interface WinningNumberFrequency {
  number: number;
  frequencyOverTime: Record<string, number>; // ✅ Key: Date, Value: Frequency
}
