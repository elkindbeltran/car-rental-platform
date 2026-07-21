import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Booking, BookingInput, Customer, CustomerInput, CustomerSummary, Page, Vehicle, VehicleInput, VehicleStatus, VehicleSummary } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  vehicles(page = 1, pageSize = 12, search = '', status?: VehicleStatus) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<Page<VehicleSummary>>(`${this.baseUrl}/vehicles`, { params });
  }
  vehicle(id: string) { return this.http.get<Vehicle>(`${this.baseUrl}/vehicles/${id}`); }
  createVehicle(value: VehicleInput) { return this.http.post<Vehicle>(`${this.baseUrl}/vehicles`, value); }
  updateVehicle(id: string, value: VehicleInput) { return this.http.put<Vehicle>(`${this.baseUrl}/vehicles/${id}`, value); }
  deleteVehicle(id: string, rowVersion: string) { return this.http.delete<void>(`${this.baseUrl}/vehicles/${id}`, { headers: { 'If-Match': `\"${rowVersion}\"` } }); }

  customers(page = 1, pageSize = 100, search = '') {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<Page<CustomerSummary>>(`${this.baseUrl}/customers`, { params });
  }
  customer(id: string) { return this.http.get<Customer>(`${this.baseUrl}/customers/${id}`); }
  createCustomer(value: CustomerInput) { return this.http.post<Customer>(`${this.baseUrl}/customers`, value); }
  updateCustomer(id: string, value: CustomerInput) { return this.http.put<Customer>(`${this.baseUrl}/customers/${id}`, value); }
  deleteCustomer(id: string, rowVersion: string) { return this.http.delete<void>(`${this.baseUrl}/customers/${id}`, { headers: { 'If-Match': `\"${rowVersion}\"` } }); }
  createBooking(value: BookingInput) { return this.http.post<Booking>(`${this.baseUrl}/bookings`, value); }
}
