import { z } from 'zod';
const required = z
  .string()
  .trim()
  .min(1, 'Обязательное поле')
  .max(255, 'Не более 255 символов');
export const formSchema = z
  .object({
    firstName: required,
    lastName: required,
    city: required,
    education: required,
    goalDescription: required,
    communicationFormat: z.coerce.number().int().min(0).max(2),
    monthsNumberMin: z.coerce.number().int().positive(),
    monthsNumberMax: z.coerce.number().int().positive(),
    hoursNumberWeek: z.coerce.number().int().min(1).max(168),
    skills: z
      .array(
        z.object({
          name: required,
          skillLevel: z.coerce.number().int().min(0).max(3),
        }),
      )
      .min(1, 'Добавьте навык'),
    availability: z.array(
      z.object({
        dayOfWeek: z.number().int().min(0).max(6),
        startTime: z.string().regex(/^\d{2}:\d{2}(:\d{2})?$/),
        endTime: z.string().regex(/^\d{2}:\d{2}(:\d{2})?$/),
      }),
    ),
    teacherInfo: z
      .object({
        placeOfWork: required,
        position: required,
        yearsExperience: z.coerce.number().int().min(0),
        maxStudents: z.coerce.number().int().positive(),
      })
      .nullable(),
  })
  .superRefine((form, context) => {
    if (form.monthsNumberMax < form.monthsNumberMin)
      context.addIssue({
        code: 'custom',
        path: ['monthsNumberMax'],
        message: 'Максимальный срок меньше минимального',
      });
    const names = form.skills.map((s) => s.name.trim().toLowerCase());
    if (new Set(names).size !== names.length)
      context.addIssue({
        code: 'custom',
        path: ['skills'],
        message: 'Каждый навык укажите один раз',
      });
    for (let day = 0; day < 7; day++) {
      const slots = form.availability
        .filter((s) => s.dayOfWeek === day)
        .sort((a, b) => a.startTime.localeCompare(b.startTime));
      if (
        slots.some(
          (s, i) =>
            s.endTime <= s.startTime ||
            (i > 0 && slots[i - 1]!.endTime > s.startTime),
        )
      )
        context.addIssue({
          code: 'custom',
          path: ['availability'],
          message: 'Временные окна пересекаются или конец не позже начала',
        });
    }
  });
export type FormValues = z.infer<typeof formSchema>;
export function blankQuestionnaire(mentor: boolean): FormValues {
  return {
    firstName: '',
    lastName: '',
    city: '',
    education: '',
    goalDescription: '',
    communicationFormat: 0,
    monthsNumberMin: 6,
    monthsNumberMax: 6,
    hoursNumberWeek: 2,
    skills: [],
    availability: [],
    teacherInfo: mentor
      ? { placeOfWork: '', position: '', yearsExperience: 0, maxStudents: 1 }
      : null,
  };
}
