import { Component, signal } from '@angular/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
    selector: 'app-about',
    imports: [CardModule, DividerModule],
    templateUrl: './about.html',
    styleUrl: './about.css'
})
export class About {
    protected readonly title = signal('O Aplikacji');
    protected readonly version = signal('1.0.0');
    protected readonly technologies = signal([
        'Angular 20',
        'PrimeNG 20',
        'TypeScript 5.9',
        'RxJS 7.8'
    ]);
}
