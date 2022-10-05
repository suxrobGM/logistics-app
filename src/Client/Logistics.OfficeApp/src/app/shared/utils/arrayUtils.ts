export class ArrayUtils {
  static remove<T>(array: T[], item: T) {
    array.splice(array.indexOf(item), 1);
  }

  static removeByIndex<T>(array: T[], index: number) {
    array.splice(index, 1);
  }
}