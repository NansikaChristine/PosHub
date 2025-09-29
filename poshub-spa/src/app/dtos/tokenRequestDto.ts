export class TokenRequestDto {
  constructor(
    public grant_Type: string = '',
    public client_Id: string = '',
    public client_Secret: string = '',
    public scope: string = ''
  ) {}
}