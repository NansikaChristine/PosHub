export class ChargeDto {
  constructor(
    public amount: number = 0,
    public displayName: string = '',
    public name: string = ''
  ) { }
}
