export class ClientsDto {
  constructor(
    public clientId: string,
    public clientName: string,
    public clientSecret: string,
    public redirectUrl: string,
    public syncUrl: string,
    public createdAt: string,
    public updatedAt: string,
    public accountId: string,
    public locationId: string,
    public connectionId: string,
    public applicationId: string,
    public accessToken: string,
    public refreshToken: string,
    public code: string,
    public authorized: boolean
  ) {}
}