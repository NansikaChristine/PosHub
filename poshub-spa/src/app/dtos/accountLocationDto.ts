export class AccountLocationDto {
  constructor(
    public accountId: string,
    public locationId: string,
    public applicationId: string,
    public connectionId: string,
    public accessToken: string,
    public refreshToken: string,
    public authorized: boolean
  ) {}
}