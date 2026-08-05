import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin, map, switchMap, take } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthzService } from '../../core/authz.service';
import { apiErrorMessage } from '../../core/error-message';
import { Booking, CustomerSummary, VehicleStatus, VehicleSummary } from '../../core/models';

@Component({
  selector: 'app-booking',
  imports: [CurrencyPipe, DatePipe, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page booking-head"><a routerLink="/vehicles">← Back to fleet</a><span class="eyebrow">NEW RESERVATION</span><h1>Let's plan your drive.</h1><p>Three details, one great trip. Your rate is locked in when you confirm.</p></section>
    <section class="page booking-layout">
      <form class="booking-form" [formGroup]="form" (ngSubmit)="submit()">
        <div class="step"><span>01</span><div><h2>Choose your car</h2><p>Select from vehicles currently ready for the road.</p></div></div>
        <label>Vehicle<select formControlName="vehicleId" (change)="selectVehicle()"><option value="">Select a vehicle</option>@for(v of vehicles();track v.id){<option [value]="v.id">{{v.year}} {{v.make}} {{v.model}} — {{v.dailyRate | currency:v.currency}}/day</option>}</select></label>
        <div class="step"><span>02</span><div><h2>Set your dates</h2><p>All times are shown in your local timezone.</p></div></div>
        <div class="two"><label>Pick-up<input type="datetime-local" formControlName="pickupAtUtc" [min]="minimumDate"></label><label>Return<input type="datetime-local" formControlName="returnAtUtc" [min]="form.controls.pickupAtUtc.value"></label></div>
        <div class="step"><span>03</span><div><h2>Who's driving?</h2><p>{{isAdmin()?'Choose the customer attached to this reservation.':'This reservation will be booked for your member profile.'}}</p></div></div>
        <label>Customer<select formControlName="customerId">@if(isAdmin()){<option value="">Select a customer</option>}@for(c of customers();track c.id){<option [value]="c.id">{{c.firstName}} {{c.lastName}} — {{c.email}}</option>}</select><small>{{isAdmin()?'Customer selection is available to administrators.':'Your identity is verified from your signed-in account.'}}</small></label>
        @if(error()){<div class="notice error">{{error()}}</div>}
        <button class="button primary confirm" [disabled]="form.invalid || submitting() || !selectedVehicle()">{{submitting()?'Confirming…':'Confirm booking'}} <span>→</span></button>
      </form>
      <aside class="summary">
        <span class="eyebrow">YOUR JOURNEY</span>
        @if(selectedVehicle();as vehicle){
          <div class="mini-car"><div></div><i></i><i></i></div><h2>{{vehicle.make}} {{vehicle.model}}</h2><span class="meta">{{vehicle.year}} · {{vehicle.licensePlate}}</span>
          <dl><div><dt>Pick-up</dt><dd>{{pickupDate() | date:'MMM d, y · h:mm a'}}</dd></div><div><dt>Return</dt><dd>{{returnDate() | date:'MMM d, y · h:mm a'}}</dd></div><div><dt>Duration</dt><dd>{{rentalDays()}} {{rentalDays()===1?'day':'days'}}</dd></div></dl>
          <div class="total"><span>Estimated total<small>{{vehicle.dailyRate | currency:vehicle.currency}} × {{rentalDays()}} days</small></span><strong>{{totalPrice() | currency:vehicle.currency:'symbol':'1.2-2'}}</strong></div>
        } @else {<div class="summary-empty"><span>⌁</span><p>Choose a vehicle to see your journey summary.</p></div>}
        <p class="fine-print">No charge is made in this demo. Availability is confirmed when your reservation is created.</p>
      </aside>
    </section>
    @if(booking();as result){<div class="modal-backdrop"><section class="modal success-modal"><span class="success-mark">✓</span><span class="eyebrow">BOOKING CONFIRMED</span><h2>Your drive is ready.</h2><p>Reservation <strong>#{{result.id.slice(0,8).toUpperCase()}}</strong> has been created for {{result.totalAmount | currency:result.currency}}.</p><a routerLink="/vehicles" class="button primary">Return to the fleet</a></section></div>}
  `,
  styleUrl: './booking.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingComponent {
  private readonly api=inject(ApiService);private readonly authz=inject(AuthzService);private readonly fb=inject(FormBuilder);private readonly route=inject(ActivatedRoute);
  readonly vehicles=signal<VehicleSummary[]>([]);readonly customers=signal<CustomerSummary[]>([]);readonly isAdmin=signal(false);readonly selectedVehicle=signal<VehicleSummary|null>(null);readonly error=signal('');readonly submitting=signal(false);readonly booking=signal<Booking|null>(null);
  readonly minimumDate=this.localInput(new Date(Date.now()+60*60*1000));
  readonly form=this.fb.nonNullable.group({vehicleId:[this.route.snapshot.queryParamMap.get('vehicle')??'',Validators.required],customerId:['',Validators.required],pickupAtUtc:[this.minimumDate,Validators.required],returnAtUtc:[this.localInput(new Date(Date.now()+25*60*60*1000)),Validators.required]});
  constructor(){const customers=this.authz.isAdmin$.pipe(take(1),switchMap(isAdmin=>{this.isAdmin.set(isAdmin);return isAdmin?this.api.customers(1,100).pipe(map(page=>page.items)):this.api.currentCustomer().pipe(map(customer=>[customer]));}));forkJoin({vehicles:this.api.vehicles(1,100,'',VehicleStatus.Available),customers}).subscribe({next:r=>{this.vehicles.set(r.vehicles.items);this.customers.set(r.customers);if(!this.isAdmin()&&r.customers.length)this.form.controls.customerId.setValue(r.customers[0].id);this.selectVehicle()},error:e=>this.error.set(apiErrorMessage(e))});this.form.valueChanges.subscribe(()=>this.error.set(''))}
  selectVehicle(){this.selectedVehicle.set(this.vehicles().find(v=>v.id===this.form.controls.vehicleId.value)??null)}
  pickupDate(){return this.form.controls.pickupAtUtc.value?new Date(this.form.controls.pickupAtUtc.value):null}returnDate(){return this.form.controls.returnAtUtc.value?new Date(this.form.controls.returnAtUtc.value):null}
  rentalDays(){const start=this.pickupDate()?.getTime()??0,end=this.returnDate()?.getTime()??0;return Math.max(1,Math.ceil((end-start)/86400000))}totalPrice(){return(this.selectedVehicle()?.dailyRate??0)*this.rentalDays()}
  submit(){const vehicle=this.selectedVehicle();if(this.form.invalid||!vehicle)return;const start=this.pickupDate()!,end=this.returnDate()!;if(end<=start){this.error.set('Return time must be later than pick-up time.');return}this.submitting.set(true);this.api.createBooking({customerId:this.form.controls.customerId.value,vehicleId:vehicle.id,pickupAtUtc:start.toISOString(),returnAtUtc:end.toISOString(),dailyRate:vehicle.dailyRate,currency:vehicle.currency}).subscribe({next:r=>{this.booking.set(r);this.submitting.set(false)},error:e=>{this.error.set(apiErrorMessage(e));this.submitting.set(false)}})}
  private localInput(date:Date){const offset=date.getTimezoneOffset();return new Date(date.getTime()-offset*60000).toISOString().slice(0,16)}
}
