import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-address-searchbox',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './address-searchbox.component.html',
  styleUrls: ['./address-searchbox.component.scss'],
})
export class AddressSearchboxComponent implements OnInit {
  @Input({required: true}) accessToken!: string;
  @Input() field: string = '';
  @Input() placeholder: string = 'Type address...';
  @Input() country: string = 'us';
  @Input() address: string = '';
  @Output() addressChange = new EventEmitter<string>();
  @Output() selectedAddress = new EventEmitter<SelectedAddressEvent>();

  public searchResults: GeocodingFeature[] = [];

  ngOnInit(): void {}

  async onAddressInputChange(event: Event) {
    const query = (event.target as HTMLInputElement)?.value;

    if (!query) {
      this.searchResults = [];
      return;
    }

    const response = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${query}.json?access_token=${this.accessToken}&country=${this.country}&types=address`);
    const responseData = await response.json() as GeocodingResponse;
    this.searchResults = responseData.features;
  }

  onClickAddress(address: string, center: number[]) {
    this.address = address;
    this.addressChange.emit(address);
    this.selectedAddress.emit({address: address, center});
    this.searchResults = [];
  }
}

interface GeocodingResponse {
  type: string;
  query: string[];
  features: GeocodingFeature[];
}

interface GeocodingFeature {
  id: string;
  address: string;
  place_name: string;
  center: number[];
}

export interface SelectedAddressEvent {
  address: string;
  center: number[];
}
