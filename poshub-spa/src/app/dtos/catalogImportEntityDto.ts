import { ProductDto } from "./productDto";

export class CatalogImportEntityDto {
  constructor(
    // public location: LocationDto = new LocationDto(),
    // public categories: CategoryDto[] = [],
    public products: ProductDto[] = [],
    // public modifierGroups: ModifierGroupDto[] = [],
    // public modifiers: ModifierDto[] = []
  ) {}
}