import { CustomThemeConfig } from '@skeletonlabs/tw-plugin';

export const myCustomTheme: CustomThemeConfig = {
	name: 'my-custom-theme',
	properties: {
		// =~= Theme Properties =~=
		'--theme-font-family-base': `ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace`,
		'--theme-font-family-heading': `system-ui`,
		'--theme-font-color-base': 'var(--color-primary-500)',
		'--theme-font-color-dark': '255 255 255',
		'--theme-rounded-base': '32px',
		'--theme-rounded-container': '5px',
		'--theme-border-base': '1px',
		// =~= Theme On-X Colors =~=
		'--on-primary': '255 255 255',
		'--on-secondary': '0 0 0',
		'--on-tertiary': '0 0 0',
		'--on-success': '255 255 255',
		'--on-warning': '0 0 0',
		'--on-error': '0 0 0',
		'--on-surface': '0 0 0',
		// =~= Theme Colors  =~=
		// primary | #4f4f4f
		'--color-primary-50': '229 229 229', // #e5e5e5
		'--color-primary-100': '220 220 220', // #dcdcdc
		'--color-primary-200': '211 211 211', // #d3d3d3
		'--color-primary-300': '185 185 185', // #b9b9b9
		'--color-primary-400': '132 132 132', // #848484
		'--color-primary-500': '79 79 79', // #4f4f4f
		'--color-primary-600': '71 71 71', // #474747
		'--color-primary-700': '59 59 59', // #3b3b3b
		'--color-primary-800': '47 47 47', // #2f2f2f
		'--color-primary-900': '39 39 39', // #272727
		// secondary | #6190c2
		'--color-secondary-50': '231 238 246', // #e7eef6
		'--color-secondary-100': '223 233 243', // #dfe9f3
		'--color-secondary-200': '216 227 240', // #d8e3f0
		'--color-secondary-300': '192 211 231', // #c0d3e7
		'--color-secondary-400': '144 177 212', // #90b1d4
		'--color-secondary-500': '97 144 194', // #6190c2
		'--color-secondary-600': '87 130 175', // #5782af
		'--color-secondary-700': '73 108 146', // #496c92
		'--color-secondary-800': '58 86 116', // #3a5674
		'--color-secondary-900': '48 71 95', // #30475f
		// tertiary | #c7d1d0
		'--color-tertiary-50': '247 248 248', // #f7f8f8
		'--color-tertiary-100': '244 246 246', // #f4f6f6
		'--color-tertiary-200': '241 244 243', // #f1f4f3
		'--color-tertiary-300': '233 237 236', // #e9edec
		'--color-tertiary-400': '216 223 222', // #d8dfde
		'--color-tertiary-500': '199 209 208', // #c7d1d0
		'--color-tertiary-600': '179 188 187', // #b3bcbb
		'--color-tertiary-700': '149 157 156', // #959d9c
		'--color-tertiary-800': '119 125 125', // #777d7d
		'--color-tertiary-900': '98 102 102', // #626666
		// success | #c6016f
		'--color-success-50': '246 217 233', // #f6d9e9
		'--color-success-100': '244 204 226', // #f4cce2
		'--color-success-200': '241 192 219', // #f1c0db
		'--color-success-300': '232 153 197', // #e899c5
		'--color-success-400': '215 77 154', // #d74d9a
		'--color-success-500': '198 1 111', // #c6016f
		'--color-success-600': '178 1 100', // #b20164
		'--color-success-700': '149 1 83', // #950153
		'--color-success-800': '119 1 67', // #770143
		'--color-success-900': '97 0 54', // #610036
		// warning | #1cb96f
		'--color-warning-50': '221 245 233', // #ddf5e9
		'--color-warning-100': '210 241 226', // #d2f1e2
		'--color-warning-200': '198 238 219', // #c6eedb
		'--color-warning-300': '164 227 197', // #a4e3c5
		'--color-warning-400': '96 206 154', // #60ce9a
		'--color-warning-500': '28 185 111', // #1cb96f
		'--color-warning-600': '25 167 100', // #19a764
		'--color-warning-700': '21 139 83', // #158b53
		'--color-warning-800': '17 111 67', // #116f43
		'--color-warning-900': '14 91 54', // #0e5b36
		// error | #d466e1
		'--color-error-50': '249 232 251', // #f9e8fb
		'--color-error-100': '246 224 249', // #f6e0f9
		'--color-error-200': '244 217 248', // #f4d9f8
		'--color-error-300': '238 194 243', // #eec2f3
		'--color-error-400': '225 148 234', // #e194ea
		'--color-error-500': '212 102 225', // #d466e1
		'--color-error-600': '191 92 203', // #bf5ccb
		'--color-error-700': '159 77 169', // #9f4da9
		'--color-error-800': '127 61 135', // #7f3d87
		'--color-error-900': '104 50 110', // #68326e
		// surface | #b0b0b0
		'--color-surface-50': '243 243 243', // #f3f3f3
		'--color-surface-100': '239 239 239', // #efefef
		'--color-surface-200': '235 235 235', // #ebebeb
		'--color-surface-300': '223 223 223', // #dfdfdf
		'--color-surface-400': '200 200 200', // #c8c8c8
		'--color-surface-500': '176 176 176', // #b0b0b0
		'--color-surface-600': '158 158 158', // #9e9e9e
		'--color-surface-700': '132 132 132', // #848484
		'--color-surface-800': '106 106 106', // #6a6a6a
		'--color-surface-900': '86 86 86' // #565656
	}
};
