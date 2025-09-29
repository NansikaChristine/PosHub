import { ProductDto } from "./productDto";

export class CatalogProductsResponseDto {
  constructor(
    public hasNextPage: boolean,
    public data: ProductDto[]
  ) {}
}
