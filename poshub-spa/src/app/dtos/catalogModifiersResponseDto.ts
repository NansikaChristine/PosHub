import { ModifierDto } from "./modifierDto";

export class CatalogModifiersResponseDto {
  constructor(
    public hasNextPage: boolean,
    public data: ModifierDto[],
    public nextPageKey: string
  ) {}
}
