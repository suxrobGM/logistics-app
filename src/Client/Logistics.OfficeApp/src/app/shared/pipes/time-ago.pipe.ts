import {Pipe, PipeTransform} from '@angular/core';

@Pipe({
  name: 'timeAgo',
  pure: false,
  standalone: true,
})
export class TimeAgoPipe implements PipeTransform {
  transform(date: string): string {
    const seconds = Math.floor((new Date().getTime() - new Date(date).getTime()) / 1000);
    let interval = Math.floor(seconds / 31536000); // 1 year

    if (interval > 1) {
      return interval + 'y';
    }

    interval = Math.floor(seconds / 2592000); // 1 month
    if (interval > 1) {
      return interval + 'M';
    }

    interval = Math.floor(seconds / 86400); // 1 day
    if (interval > 1) {
      return interval + 'd';
    }

    interval = Math.floor(seconds / 3600); // 1 hour
    if (interval > 1) {
      return interval + 'h';
    }

    interval = Math.floor(seconds / 60); // 1 minute
    if (interval > 1) {
      return interval + 'm';
    }
    return seconds + 's';
  }
}
