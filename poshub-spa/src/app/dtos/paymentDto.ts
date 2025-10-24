export class PaymentDto {
  constructor(
    public amount: number = 0,
    public name: string = ''
  ) { }
}