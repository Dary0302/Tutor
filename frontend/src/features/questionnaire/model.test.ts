import { expect, it } from 'vitest';
import { blankQuestionnaire, formSchema } from './model';
const valid = () => ({
  ...blankQuestionnaire(false),
  firstName: 'Анна',
  lastName: 'Иванова',
  city: 'Москва',
  education: 'Вуз',
  goalDescription: 'Цель',
  skills: [{ name: 'TypeScript', skillLevel: 1 }],
});
it('validates role-specific defaults and coercion', () => {
  expect(blankQuestionnaire(true).teacherInfo).toMatchObject({
    maxStudents: 1,
  });
  expect(blankQuestionnaire(false).teacherInfo).toBeNull();
  expect(formSchema.parse(valid()).monthsNumberMax).toBe(6);
  expect(formSchema.safeParse(blankQuestionnaire(false)).success).toBe(false);
});
it('rejects invalid ranges, duplicated trimmed skills and intersecting windows', () => {
  expect(formSchema.safeParse({ ...valid(), monthsNumberMax: 1 }).success).toBe(
    false,
  );
  expect(
    formSchema.safeParse({
      ...valid(),
      skills: [
        { name: ' TypeScript ', skillLevel: 0 },
        { name: 'typescript', skillLevel: 1 },
      ],
    }).success,
  ).toBe(false);
  for (const availability of [
    [{ dayOfWeek: 1, startTime: '20:00', endTime: '18:00' }],
    [
      { dayOfWeek: 1, startTime: '18:00', endTime: '20:00' },
      { dayOfWeek: 1, startTime: '19:00', endTime: '21:00' },
    ],
  ])
    expect(formSchema.safeParse({ ...valid(), availability }).success).toBe(
      false,
    );
});
it('accepts adjacent intervals and different days', () => {
  const availability = [
    { dayOfWeek: 1, startTime: '18:00', endTime: '19:00' },
    { dayOfWeek: 1, startTime: '19:00', endTime: '20:00' },
    { dayOfWeek: 2, startTime: '18:00', endTime: '19:00' },
  ];
  expect(formSchema.safeParse({ ...valid(), availability }).success).toBe(true);
});
