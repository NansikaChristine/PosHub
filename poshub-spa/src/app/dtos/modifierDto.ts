export class ModifierDto {
   constructor(
  public posReference: string = '',
  public name: string = '',
  public description: string = '',
  public posVersion: string = '',
  public originalImageUrl: string = '',
  public price: number = 0,
  public inStorePrice?: number,
  public taxRate: number = 0,
  public isTaxIncluded: boolean = true,
  public containsAlcohol: boolean = false,
  public containsTobacco: boolean = false,
  public isBikeFriendly: boolean = false,
  public showOnline: boolean = false,
  public position: number = 0,
  public minPermitted: number = 0,
  public maxPermitted: number = 0,
    //public nutritionalInfo: NutritionalInfoDto = new NutritionalInfoDto(),
   // public selections: SelectionDto[] = []
  public accountId: string = '',
  public createdAt: Date = new Date(),
  public taxRateIds: string[] = [],
  public locationId: string = '',
  public imageUrl: string = '',
  public updatedAt: Date = new Date()
     ) {}
}
