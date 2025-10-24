export class CustomerDto {
  constructor(
    public firstName: string = '',
    public lastName: string = '',
    public phone: string = '',
    public phonePin: string = '',
    public id: string = '',
    public email: string = ''
  ) { }
}
