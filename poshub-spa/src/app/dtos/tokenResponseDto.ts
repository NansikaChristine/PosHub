export class TokenResponseDto {
  constructor(
    public tokenType: string = '',
    public expiresIn: number,
    public access_token: string = '',
    public refresh_token: string = '',
    public scope: string = '',
    public errorMessage: string = ''
  ) {}
}