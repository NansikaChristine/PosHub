export class DiscountDto {
  constructor(
    public amount: number = 0,
    public displayName: string = '',
    public name: string = '',
    public type: string = ''
  ) { }
}
